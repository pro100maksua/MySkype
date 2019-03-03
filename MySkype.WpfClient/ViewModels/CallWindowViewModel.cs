using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AForge.Video;
using AForge.Video.DirectShow;
using MySkype.WpfClient.ApiInterfaces;
using MySkype.WpfClient.Models;
using MySkype.WpfClient.Services;
using Nito.Mvvm;
using ReactiveUI;
using RestEase;

namespace MySkype.WpfClient.ViewModels
{
    class CallWindowViewModel : ViewModelBase
    {
        private readonly ICallsApi _callsClient;
        private readonly User _user;
        private ObservableCollection<User> _participants;
        private readonly WebSocketClient _webSocketClient;
        private readonly NotificationService _notificationService;
        private readonly CallService _callService;
        private readonly VideoCaptureDevice _webCam;
        private readonly DispatcherTimer _timer;
        private TimeSpan _duration = TimeSpan.Zero;
        private bool _started;
        private bool _paused;
        private bool _muted;
        private ObservableCollection<ChatMessage> _messages = new ObservableCollection<ChatMessage>();
        private string _message;
        private bool _isChatEnabled;
        private BitmapImage _frame;
        private readonly WebSocketClient _webSocketVideoClient;
        private bool _videoPlaying;
        private bool _isAddingFriendToCall;
        private ObservableCollection<User> _friends = new ObservableCollection<User>();

        private ObservableCollection<KeyValuePair<Guid, BitmapImage>> _frames =
            new ObservableCollection<KeyValuePair<Guid, BitmapImage>>();
        private User _chosenUser;

        public bool IsCaller { get; }
        public bool Started
        {
            get => _started;
            set => this.RaiseAndSetIfChanged(ref _started, value);
        }
        public bool IsChatEnabled
        {
            get => _isChatEnabled;
            set => this.RaiseAndSetIfChanged(ref _isChatEnabled, value);
        }
        public bool IsAddingFriendToCall
        {
            get => _isAddingFriendToCall;
            set => this.RaiseAndSetIfChanged(ref _isAddingFriendToCall, value);
        }
        public User Friend { get; }
        public User ChosenUser
        {
            get => _chosenUser;
            set => this.RaiseAndSetIfChanged(ref _chosenUser, value);
        }
        public TimeSpan Duration
        {
            get => _duration;
            set => this.RaiseAndSetIfChanged(ref _duration, value);
        }
        public ObservableCollection<User> Friends
        {
            get => _friends;
            set => this.RaiseAndSetIfChanged(ref _friends, value);
        }
        public ObservableCollection<ChatMessage> Messages
        {
            get => _messages;
            set => this.RaiseAndSetIfChanged(ref _messages, value);
        }
        public ObservableCollection<User> Participants
        {
            get => _participants;
            set => this.RaiseAndSetIfChanged(ref _participants, value);
        }
        public ObservableCollection<KeyValuePair<Guid, BitmapImage>> Frames
        {
            get => _frames;
            set => this.RaiseAndSetIfChanged(ref _frames, value);
        }
        public string Message
        {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }
        public BitmapImage Frame
        {
            get => _frame;
            set => this.RaiseAndSetIfChanged(ref _frame, value);
        }

        public AsyncCommand ToggleChatCommand { get; set; }
        public AsyncCommand SendMessageCommand { get; set; }
        public AsyncCommand CloseCommand { get; }
        public AsyncCommand ToggleRecordingCommand { get; set; }
        public AsyncCommand TogglePlayingCommand { get; set; }
        public AsyncCommand ToggleVideoCommand { get; set; }
        public AsyncCommand ShowFriendsCommand { get; set; }
        public AsyncCommand AddFriendToCallCommand { get; set; }

        public CallWindowViewModel(User user, List<User> friends, User friend, WebSocketClient webSocketClient,
            string token, NotificationService notificationService, bool isCaller)
        {
            Friend = friend;
            IsCaller = isCaller;
            _user = user;

            _callsClient = RestClient.For<ICallsApi>("http://localhost:5000/api/calls");
            _callsClient.Token = "Bearer " + token;

            GetParticipantsAsync().ContinueWith(t =>
            {
                return ChosenUser = Participants.FirstOrDefault(p => p.Id != user.Id);
            });

            _videoPlaying = true;
            _webSocketClient = webSocketClient;
            _notificationService = notificationService;
            Friends = new ObservableCollection<User>(friends);

            Started = !isCaller;

            _callService = new CallService(webSocketClient);

            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };
            _timer.Tick += (sender, e) => { Duration = Duration.Add(TimeSpan.FromSeconds(1)); };
            _webSocketVideoClient = new WebSocketClient(notificationService, token, "video");
            _webSocketVideoClient.Start();
            _webSocketClient.MessageReceived += OnMessageReceived;
            _webSocketVideoClient.DataReceived += FrameReceived;
            _notificationService.CallRejected += OnCallRejected;
            _notificationService.CallEnded += OnCallEnded;
            _notificationService.CallAccepted += OnFriendAddedToCall;

            _webCam = new VideoCaptureDevice(new FilterInfoCollection(FilterCategory.VideoInputDevice)[0]
                .MonikerString);
            _webCam.NewFrame += NewWebCamFrame;

            ToggleChatCommand = new AsyncCommand(async () => await
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(() => IsChatEnabled = !IsChatEnabled)));
            ShowFriendsCommand = new AsyncCommand(async () => await
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(() => IsAddingFriendToCall = true)));
            SendMessageCommand = new AsyncCommand(SendMessageAsync);
            CloseCommand = new AsyncCommand(() => FinishCallAsync(false));
            TogglePlayingCommand = new AsyncCommand(TogglePlayingAsync);
            ToggleRecordingCommand = new AsyncCommand(ToggleRecordingAsync);
            ToggleVideoCommand = new AsyncCommand(ToggleVideoAsync);
            AddFriendToCallCommand = new AsyncCommand(AddFriendToCallAsync);

            if (Started)
            {
                StartCall();
            }
            else
            {
                _notificationService.CallAccepted += OnCallAccepted;
            }
        }

        private async Task GetParticipantsAsync()
        {
            _participants = new ObservableCollection<User> { _user };

            if (!IsCaller)
            {
                var participantIds = await _callsClient.GetCallParticipantsAsync(Friend.Id);

                foreach (var id in participantIds)
                {
                    if (_user.Id == id) continue;

                    var user = Friends.FirstOrDefault(f => f.Id == id);
                    Participants.Add(user);
                }
            }
        }

        private void OnFriendAddedToCall(object sender, MyEventArgs e)
        {
            var friend = Friends.FirstOrDefault(f => f.Id == e.SenderId);

            Application.Current.Dispatcher.Invoke(() => Participants.Add(friend));

            if (ChosenUser == null)
            {
                ChosenUser = friend;
            }
        }

        private async Task AddFriendToCallAsync(object arg)
        {
            if (arg is User friend)
            {
                IsAddingFriendToCall = false;

                await _webSocketClient.SendNotificationAsync(friend.Id, NotificationType.CallRequest);
            }
        }

        private void FrameReceived(object sender, DataReceivedEventArgs e)
        {
            var image = new BitmapImage();

            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = new MemoryStream(e.Data);
            image.EndInit();

            image.Freeze();

            Application.Current.Dispatcher.Invoke(() =>
            {
                var pair = Frames.SingleOrDefault(p => p.Key == e.SenderId);

                if (pair.Equals(default(KeyValuePair<Guid, BitmapImage>)))
                {
                    Frames.Add(new KeyValuePair<Guid, BitmapImage>(e.SenderId, image));
                }
                else
                {
                    var index = Frames.IndexOf(pair);
                    Frames[index] = new KeyValuePair<Guid, BitmapImage>(e.SenderId, image);
                }
            });
        }

        private async void NewWebCamFrame(object sender, NewFrameEventArgs eventArgs)
        {
            using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
            {
                var memory = new MemoryStream();
                bitmap.Save(memory, ImageFormat.Jpeg);
                var bytes = memory.ToArray();

                await _webSocketVideoClient.SendDataAsync(bytes);
            }
        }

        private async Task SendMessageAsync()
        {
            await _webSocketClient.SendMessageAsync(Friend.Id, Message);

            Message = string.Empty;
        }

        private async void OnMessageReceived(object sender, ChatMessageReceivedEventArgs e)
        {
            var chatMessage = new ChatMessage { Content = e.Content };

            var user = _participants.FirstOrDefault(p => p.Id == e.SenderId);
            chatMessage.UserName = user.FirstName;
            chatMessage.UserAvatar = user.Avatar.Bitmap;

            await Application.Current.Dispatcher.BeginInvoke(new Action(() => { Messages.Add(chatMessage); }));
        }

        public async Task TogglePlayingAsync()
        {
            await Task.Run(() =>
            {
                if (_paused)
                {
                    _callService.ContinuePlaying();
                }
                else
                {
                    _callService.PausePlaying();
                }

                _paused = !_paused;
            });
        }

        private async Task ToggleVideoAsync()
        {
            await Task.Run(() =>
            {
                if (_videoPlaying)
                {
                    _webCam.NewFrame -= NewWebCamFrame;
                    _webCam.Stop();
                }
                else
                {
                    _webCam.NewFrame += NewWebCamFrame;
                    _webCam.Start();
                }

                _videoPlaying = !_videoPlaying;
            });
        }

        public async Task ToggleRecordingAsync()
        {
            await Task.Run(() =>
            {
                if (_muted)
                {
                    _callService.ContinueRecording();
                }
                else
                {
                    _callService.PauseRecording();
                }

                _muted = !_muted;
            });
        }

        private async void OnCallEnded(object sender, MyEventArgs e)
        {
            await FinishCallAsync(true);
        }

        private async void OnCallRejected(object sender, MyEventArgs e)
        {
            await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(RequestClose));
        }

        private void OnCallAccepted(object sender, MyEventArgs e)
        {
            if (Friend.Id != e.SenderId) return;

            _notificationService.CallAccepted -= OnCallAccepted;

            Started = true;
            StartCall();
        }

        public event EventHandler CloseRequested;

        private void RequestClose()
        {
            CloseRequested?.Invoke(this, new EventArgs());
        }

        public void StartCall()
        {
            _timer.Start();

            _webCam.Start();

            _callService.StartCall();
        }

        private async Task FinishCallAsync(bool requested)
        {
            await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(async () =>
            {
                RequestClose();

                await StopCallAsync(requested);
            }));
        }

        public async Task StopCallAsync(bool requested)
        {
            _webSocketClient.MessageReceived -= OnMessageReceived;
            _webSocketVideoClient.DataReceived -= FrameReceived;
            
            if (_webCam.IsRunning) _webCam.SignalToStop();

            if (_timer.IsEnabled) _timer.Stop();

            if (!requested)
                await _webSocketClient.SendNotificationAsync(Friend.Id, NotificationType.CallEnded);

            if (IsCaller)
                SaveCallInfo();

            _webSocketVideoClient.CloseConnection();
            _callService.StopCall();
        }

        public void SaveCallInfo()
        {
            Task.Run(async () =>
            {
                var call = new Call
                {
                    Duration = _duration.Ticks,
                    StartTime = DateTime.Now.Subtract(_duration).Ticks,
                    ParticipantIds = new List<Guid> { Friend.Id, _user.Id }
                };

                await _callsClient.SaveCallInfoAsync(call);
            });
        }
    }

    internal class ChatMessage
    {
        public string Content { get; set; }

        public string UserName { get; set; }

        public BitmapImage UserAvatar { get; set; }
    }
}