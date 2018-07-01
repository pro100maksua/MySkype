using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AForge.Video;
using AForge.Video.DirectShow;
using MySkype.WpfClient.Models;
using MySkype.WpfClient.Services;
using Nito.Mvvm;
using ReactiveUI;
using WebSocket4Net;

namespace MySkype.WpfClient.ViewModels
{
    class CallWindowViewModel : ViewModelBase
    {
        private readonly bool _isCaller;
        private readonly User _user;

        private readonly WebSocketClient _webSocketClient;
        private readonly RestSharpClient _restClient;
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
        private WebSocketClient _webSocketVideoClient;
        private bool _videoPlaying;

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
        public User Friend { get; }
        public TimeSpan Duration
        {
            get => _duration;
            set => this.RaiseAndSetIfChanged(ref _duration, value);
        }
        public ObservableCollection<ChatMessage> Messages
        {
            get => _messages;
            set => this.RaiseAndSetIfChanged(ref _messages, value);
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

        public CallWindowViewModel(User user, User friend, WebSocketClient webSocketClient, string token, RestSharpClient restClient, NotificationService notificationService, bool isCaller)
        {
            _videoPlaying = true;
            _user = user;
            _webSocketClient = webSocketClient;
            _restClient = restClient;
            _notificationService = notificationService;
            Friend = friend;
            _isCaller = isCaller;
            Started = !isCaller;

            _callService = new CallService(webSocketClient, Friend.Id);

            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };
            _timer.Tick += (sender, e) => { Duration = Duration.Add(TimeSpan.FromSeconds(1)); };
            _webSocketVideoClient = new WebSocketClient(notificationService, token, "video");
            _webSocketVideoClient.Start();
            _webSocketClient.MessageReceived += OnMessageReceived;
            _webSocketVideoClient.DataReceived += FrameReceived;
            _notificationService.CallRejected += OnCallRejected;
            _notificationService.CallEnded += OnCallEnded;

            _webCam = new VideoCaptureDevice(new FilterInfoCollection(FilterCategory.VideoInputDevice)[0]
                .MonikerString);
            _webCam.NewFrame += NewWebCamFrame;

            ToggleChatCommand = new AsyncCommand(async () => await
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new Action(() => IsChatEnabled = !IsChatEnabled)));
            SendMessageCommand = new AsyncCommand(SendMessageAsync);
            CloseCommand = new AsyncCommand(() => FinishCallAsync(false));
            TogglePlayingCommand = new AsyncCommand(TogglePlayingAsync);
            ToggleRecordingCommand = new AsyncCommand(ToggleRecordingAsync);
            ToggleVideoCommand = new AsyncCommand(ToggleVideoAsync);

            if (Started)
            {
                StartCall();
            }
            else
            {
                _notificationService.CallAccepted += OnCallAccepted;
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

            Application.Current.Dispatcher.Invoke(() => Frame = image);
        }

        private async void NewWebCamFrame(object sender, NewFrameEventArgs eventargs)
        {
            using (var bitmap = (Bitmap)eventargs.Frame.Clone())
            {
                var memory = new MemoryStream();
                bitmap.Save(memory, ImageFormat.Jpeg);
                await _webSocketVideoClient.SendDataAsync(Friend.Id, memory.ToArray());
            }
        }

        private async Task SendMessageAsync()
        {
            await _webSocketClient.SendMessageAsync(Friend.Id, Message);
            await _webSocketClient.SendMessageAsync(_user.Id, Message);

            Message = string.Empty;
        }

        private async void OnMessageReceived(object sender, ChatMessageReceivedEventArgs e)
        {
            var chatMessage = new ChatMessage { Content = e.Content };

            if (_user.Id == e.SenderId)
            {
                chatMessage.UserName = _user.FirstName;
                chatMessage.UserAvatar = _user.Avatar.Bitmap;
            }
            else if (Friend.Id == e.SenderId)
            {
                chatMessage.UserName = Friend.FirstName;
                chatMessage.UserAvatar = Friend.Avatar.Bitmap;
            }

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
                    _webSocketVideoClient.DataReceived -= FrameReceived;
                    Frame = null;
                }
                else
                {
                    _webSocketVideoClient.DataReceived += FrameReceived;
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
            if (_webCam.IsRunning) _webCam.SignalToStop();

            if (_timer.IsEnabled) _timer.Stop();

            if (!requested)
                await _webSocketClient.SendNotificationAsync(Friend.Id, NotificationType.CallEnded);

            _callService.StopCall();

            if (_isCaller)
                SaveCallInfo();
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

                await _restClient.SaveCallInfoAsync(call);
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