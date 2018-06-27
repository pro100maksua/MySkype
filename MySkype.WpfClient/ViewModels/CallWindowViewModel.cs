using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MySkype.WpfClient.Models;
using MySkype.WpfClient.Services;
using Nito.Mvvm;
using ReactiveUI;

namespace MySkype.WpfClient.ViewModels
{
    class CallWindowViewModel : ViewModelBase
    {
        private readonly Guid _userId;
        private readonly bool _isCaller;
        private readonly WebSocketClient _webSocketClient;
        private readonly RestSharpClient _restClient;
        private readonly NotificationService _notificationService;
        private readonly CallService _callService;
        private readonly DispatcherTimer _timer;
        private TimeSpan _duration = TimeSpan.Zero;
        private bool _started;
        private bool _paused;
        private bool _muted;

        public bool Started
        {
            get => _started;
            set => this.RaiseAndSetIfChanged(ref _started, value);
        }
        public User Friend { get; }
        public TimeSpan Duration
        {
            get => _duration;
            set => this.RaiseAndSetIfChanged(ref _duration, value);
        }

        public AsyncCommand CloseCommand { get; }
        public AsyncCommand ToggleRecordingCommand { get; set; }
        public AsyncCommand TogglePlayingCommand { get; set; }

        public CallWindowViewModel(Guid userId, User friend, WebSocketClient webSocketClient, RestSharpClient restClient, NotificationService notificationService, bool isCaller)
        {
            _userId = userId;
            _webSocketClient = webSocketClient;
            _restClient = restClient;
            _notificationService = notificationService;
            Friend = friend;
            _isCaller = isCaller;
            Started = !isCaller;

            _callService = new CallService(webSocketClient, Friend.Id);

            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };
            _timer.Tick += (sender, e) => { Duration = Duration.Add(TimeSpan.FromSeconds(1)); };
            _notificationService.CallRejected += OnCallRejected;
            _notificationService.CallEnded += OnCallEnded;

            CloseCommand = new AsyncCommand(() => FinishCallAsync(false));
            TogglePlayingCommand = new AsyncCommand(TogglePlayingAsync);
            ToggleRecordingCommand = new AsyncCommand(ToggleRecordingAsync);

            if (Started)
            {
                StartCall();
            }
            else
            {
                _notificationService.CallAccepted += OnCallAccepted;
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

            _callService.StartCall();
        }

        private async Task FinishCallAsync(bool requested)
        {
            await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                RequestClose();

                StopCall(requested);
            }));
        }

        public void StopCall(bool requested)
        {
            if (_timer.IsEnabled)
                _timer.Stop();

            if (!requested)
                _webSocketClient.SendNotificationAsync(Friend.Id, NotificationType.CallEnded);

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
                    ParticipantIds = new List<Guid> { Friend.Id, _userId }
                };

                await _restClient.SaveCallInfoAsync(call);
            });
        }
    }
}