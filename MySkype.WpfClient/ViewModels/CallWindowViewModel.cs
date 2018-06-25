using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MySkype.WpfClient.Models;
using MySkype.WpfClient.Services;
using ReactiveUI;

namespace MySkype.WpfClient.ViewModels
{
    class CallWindowViewModel : ViewModelBase
    {
        private readonly Guid _userId;
        private readonly WebSocketClient _webSocketClient;
        private readonly RestSharpClient _restClient;
        private readonly NotificationService _notificationService;
        private readonly CallService _callService;

        private readonly DispatcherTimer _timer;
        private TimeSpan _duration = TimeSpan.Zero;
        private bool _isCaller;
        private bool _started;

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

        public CallWindowViewModel(Guid userId, User friend, WebSocketClient webSocketClient, RestSharpClient restClient, NotificationService notificationService, bool isCaller)
        {
            _userId = userId;
            _webSocketClient = webSocketClient;
            _restClient = restClient;
            _notificationService = notificationService;
            Friend = friend;
            _isCaller = isCaller;
            Started = !isCaller;

            _callService = new CallService(webSocketClient, restClient, Friend.Id);

            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };
            _timer.Tick += (sender, e) => { Duration = Duration.Add(TimeSpan.FromSeconds(1)); };
            _notificationService.CallRejected += OnCallRejected;
            _notificationService.CallEnded += StopCallAsync;

            if (Started)
            {
                StartCall();
            }
            else
            {
                _notificationService.CallAccepted += StartCall;
            }
        }

        private async void StopCallAsync(object sender, MyEventArgs e)
        {
            await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                StopCall(true);

                OnCloseRequested();
            }));
        }

        private async void OnCallRejected(object sender, MyEventArgs e)
        {
            await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(OnCloseRequested));
        }

        public event EventHandler CloseRequested;

        public void OnCloseRequested()
        {
            CloseRequested?.Invoke(this, new EventArgs());
        }

        private void StartCall(object sender, MyEventArgs e)
        {
            if (Friend.Id != e.SenderId) return;

            _notificationService.CallAccepted -= StartCall;

            Started = true;
            StartCall();
        }

        public void StartCall()
        {
            _timer.Start();

            _callService.StartCall();
        }

        public void StopCall(bool requested = false)
        {
            if (_timer.IsEnabled)
                _timer.Stop();

            if (!requested)
                _webSocketClient.SendMessage(Friend.Id, MessageType.CallEnded);

            _callService.StopCall();

            if (_isCaller)
                SaveCallInfoAsync();
        }

        public async Task SaveCallInfoAsync()
        {
            var call = new Call
            {
                Duration = _duration.Ticks,
                StartTime = DateTime.Now.Subtract(_duration).Ticks,
                ParticipantIds = new List<Guid> { Friend.Id, _userId }
            };

            await _restClient.SaveCallInfoAsync(call);
        }
    }
}