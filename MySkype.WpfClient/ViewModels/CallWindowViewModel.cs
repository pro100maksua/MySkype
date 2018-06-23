using System;
using System.Windows.Threading;
using MySkype.WpfClient.Models;
using MySkype.WpfClient.Services;
using ReactiveUI;

namespace MySkype.WpfClient.ViewModels
{
    class CallWindowViewModel : ViewModelBase
    {
        private readonly NotificationService _notificationService;
        private readonly CallService _callService;

        private readonly DispatcherTimer _timer;
        private TimeSpan _duration = TimeSpan.Zero;
        private bool _started;

        public bool Started
        {
            get => _started;
            set => this.RaiseAndSetIfChanged(ref _started, value);
        }
        public User Friend { get; set; }
        public TimeSpan Duration
        {
            get => _duration;
            set => this.RaiseAndSetIfChanged(ref _duration, value);
        }

        public CallWindowViewModel(User friend, WebSocketClient webSocketClient, RestSharpClient restClient, NotificationService notificationService, bool started)
        {
            _notificationService = notificationService;
            Friend = friend;
            Started = started;


            _callService = new CallService(webSocketClient, restClient, Friend.Id);

            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };
            _timer.Tick += (sender, e) => { Duration = Duration.Add(TimeSpan.FromSeconds(1)); };

            if (Started)
            {
                StartCall();
            }
            else
            {
                _notificationService.CallAccepted += StartCall;
            }
        }

        private void StartCall(object sender, MyEventArgs e)
        {
            _notificationService.CallAccepted -= StartCall;

            Started = true;
            StartCall();
        }

        public void StartCall()
        {
            _timer.Start();

            _callService.StartCall();
        }

        public void StopCall()
        {
            if (_timer.IsEnabled)
                _timer.Stop();

            _callService.StopCall();
        }
    }
}