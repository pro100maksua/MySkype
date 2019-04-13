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
        private readonly CallService _callService;
        private readonly WebSocketClient _webSocketClient;

        private readonly WebSocketClient _webSocketVideoClient;
        private readonly NotificationService _notificationService;
        private VideoCaptureDevice _webCam;

        private readonly User _user;
        private readonly List<User> _participants = new List<User>();
        private DispatcherTimer _timer;
        private bool _paused;
        private bool _muted;
        private bool _videoPlaying;

        private bool _started;
        private bool _isChatOpened;
        private bool _addingFriendToCall;
        private TimeSpan _duration = TimeSpan.Zero;
        private string _message;
        private ObservableCollection<ChatMessage> _messages = new ObservableCollection<ChatMessage>();
        private ObservableCollection<User> _friends = new ObservableCollection<User>();
        private ObservableCollection<KeyValuePair<Guid, BitmapImage>> _frames =
            new ObservableCollection<KeyValuePair<Guid, BitmapImage>>();

        public bool IsCaller { get; }
        public User Friend { get; }
        public bool Started
        {
            get => _started;
            set => this.RaiseAndSetIfChanged(ref _started, value);
        }
        public bool IsChatOpened
        {
            get => _isChatOpened;
            set => this.RaiseAndSetIfChanged(ref _isChatOpened, value);
        }
        public bool AddingFriendToCall
        {
            get => _addingFriendToCall;
            set => this.RaiseAndSetIfChanged(ref _addingFriendToCall, value);
        }
        public TimeSpan Duration
        {
            get => _duration;
            set => this.RaiseAndSetIfChanged(ref _duration, value);
        }
        public string Message
        {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }
        public ObservableCollection<ChatMessage> Messages
        {
            get => _messages;
            set => this.RaiseAndSetIfChanged(ref _messages, value);
        }
        public ObservableCollection<User> Friends
        {
            get => _friends;
            set => this.RaiseAndSetIfChanged(ref _friends, value);
        }
        public ObservableCollection<KeyValuePair<Guid, BitmapImage>> Frames
        {
            get => _frames;
            set => this.RaiseAndSetIfChanged(ref _frames, value);
        }

        public AsyncCommand InitCommand { get; set; }
        public AsyncCommand ToggleChatCommand { get; set; }
        public AsyncCommand SendMessageCommand { get; set; }
        public AsyncCommand CloseCommand { get; set; }
        public AsyncCommand ToggleRecordingCommand { get; }
        public AsyncCommand TogglePlayingCommand { get; set; }
        public AsyncCommand ToggleVideoCommand { get; set; }
        public AsyncCommand ShowFriendsCommand { get; set; }
        public AsyncCommand AddFriendToCallCommand { get; set; }

        public event EventHandler CloseRequested;

        public CallWindowViewModel(User user, List<User> friends, User friend, WebSocketClient webSocketClient,
            string token, NotificationService notificationService, bool isCaller)
        {
            Friends = new ObservableCollection<User>(friends);
            Friend = friend;
            _user = user;
            Started = !isCaller;
            IsCaller = isCaller;

            _webSocketClient = webSocketClient;
            _notificationService = notificationService;
            _webSocketVideoClient = new WebSocketClient(notificationService, token, "video");
            _callService = new CallService(webSocketClient);
            _callsClient = RestClient.For<ICallsApi>("http://localhost:5000/api/calls");
            _callsClient.Token = "Bearer " + token;

            InitCommand = new AsyncCommand(InitAsync);
            SendMessageCommand = new AsyncCommand(SendMessageAsync);
            AddFriendToCallCommand = new AsyncCommand(AddFriendToCallAsync);
            TogglePlayingCommand = new AsyncCommand(TogglePlayingAsync);
            ToggleRecordingCommand = new AsyncCommand(ToggleRecordingAsync);
            ToggleVideoCommand = new AsyncCommand(ToggleVideoAsync);
            CloseCommand = new AsyncCommand(() => FinishCallAsync(false));
            ToggleChatCommand = new AsyncCommand(() => Task.Run(new Action(() => IsChatOpened = !IsChatOpened)));
            ShowFriendsCommand = new AsyncCommand(() => Task.Run(new Action(() => AddingFriendToCall = !AddingFriendToCall)));
        }

        public async Task InitAsync()
        {
            await GetParticipantsAsync();

            _videoPlaying = true;

            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };
            _timer.Tick += (sender, e) => { Duration = Duration.Add(TimeSpan.FromSeconds(1)); };

            _webSocketVideoClient.Start();
            _webSocketClient.MessageReceived += OnMessageReceived;
            _webSocketVideoClient.DataReceived += FrameReceived;
            _notificationService.CallRejected += OnCallRejected;
            _notificationService.CallEnded += async (sender, e) => await FinishCallAsync(true);
            _notificationService.CallAccepted += OnFriendAddedToCall;

            _webCam = new VideoCaptureDevice(
                new FilterInfoCollection(FilterCategory.VideoInputDevice)[0].MonikerString);
            _webCam.NewFrame += NewWebCamFrame;

            if (Started)
            {
                StartCall();
            }
            else
            {
                _notificationService.CallAccepted += OnCallAccepted;
            }
        }

        private async Task SendMessageAsync()
        {
            await _webSocketClient.SendMessageAsync(Message);

            Message = string.Empty;
        }

        private async Task AddFriendToCallAsync(object arg)
        {
            if (arg is User friend)
            {
                AddingFriendToCall = !AddingFriendToCall;

                await _webSocketClient.SendNotificationAsync(friend.Id, NotificationType.CallRequest);
            }
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

        private async Task FinishCallAsync(bool requested)
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                RequestClose();

                await StopCallAsync(requested);
            });
        }

        private async Task GetParticipantsAsync()
        {
            if (!IsCaller)
            {
                var participantIds = await _callsClient.GetCallParticipantsAsync(_user.Id);
                foreach (var id in participantIds)
                {
                    var user = Friends.FirstOrDefault(f => f.Id == id);
                    if (user != null)
                    {
                        _participants.Add(user);
                    }
                }
            }

            _participants.Add(_user);
        }

        private void OnFriendAddedToCall(object sender, MyEventArgs e)
        {
            var friend = Friends.FirstOrDefault(f => f.Id == e.SenderId);

            _participants.Add(friend);
        }
        private void FrameReceived(Data data)
        {
            var image = new BitmapImage();

            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = new MemoryStream(data.Bytes);
            image.EndInit();

            image.Freeze();

            Application.Current.Dispatcher.Invoke(() =>
            {
                var pair = Frames.SingleOrDefault(p => p.Key == data.SenderId);

                if (pair.Key == Guid.Empty)
                {
                    Frames.Add(new KeyValuePair<Guid, BitmapImage>(data.SenderId, image));
                }
                else
                {
                    var index = Frames.IndexOf(pair);
                    Frames[index] = new KeyValuePair<Guid, BitmapImage>(data.SenderId, image);
                }
            });
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

                if (pair.Key == Guid.Empty)
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

        private async void OnMessageReceived(object sender, ChatMessageReceivedEventArgs e)
        {
            var chatMessage = new ChatMessage { Content = e.Content };

            var user = _participants.FirstOrDefault(p => p.Id == e.SenderId);
            chatMessage.UserName = user.FirstName;
            chatMessage.UserAvatar = user.Avatar.Bitmap;

            await Application.Current.Dispatcher.InvokeAsync(() => Messages.Add(chatMessage));
        }

        private async void OnCallRejected(object sender, MyEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync(RequestClose);
        }

        private void RequestClose()
        {
            CloseRequested?.Invoke(this, new EventArgs());
        }

        private void OnCallAccepted(object sender, MyEventArgs e)
        {
            _notificationService.CallAccepted -= OnCallAccepted;

            StartCall();
        }

        public void StartCall()
        {
            Started = true;

            _timer.Start();
            _webCam.Start();
            _callService.StartCall();
        }

        private async Task StopCallAsync(bool requested)
        {
            _webSocketClient.MessageReceived -= OnMessageReceived;
            _webSocketVideoClient.DataReceived -= FrameReceived;
            _webSocketVideoClient.CloseConnection();
            _callService.StopCall();

            if (_webCam.IsRunning) _webCam.SignalToStop();
            if (_timer.IsEnabled) _timer.Stop();
            if (!requested)
            {
                await SaveCallInfoAsync();
                await _webSocketClient.SendNotificationAsync(_user.Id, NotificationType.CallEnded);
            }
        }

        private async Task SaveCallInfoAsync()
        {
            var call = new Call
            {
                Duration = _duration.Ticks,
                StartTime = DateTime.Now.Subtract(_duration).Ticks,
                ParticipantIds = _participants.Select(u => u.Id).ToList()
            };

            await _callsClient.SaveCallInfoAsync(call);
        }
    }
}