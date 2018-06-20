using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using MySkype.Client.Models;
using MySkype.Client.Services;
using ReactiveUI;

namespace MySkype.Client.ViewModels
{
    class CallWindowViewModel : ViewModelBase
    {
        private readonly RestSharpClient _restClient;
        private CallService _callService;

        private DispatcherTimer _timer;
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
            Friend = friend;
            Started = started;

            _restClient = restClient;
            _callService = new CallService(webSocketClient);

            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal,
                (sender, e) =>
                {
                    Duration = Duration.Add(TimeSpan.FromSeconds(1));
                });

            if (Started)
            {
                StartCallAsync();
            }
            else
            {
                notificationService.CallAccepted += (sender, e) =>
                {
                    Started = true;
                    StartCallAsync();
                };
            }
        }

        public async Task StartCallAsync()
        {
            _timer.Start();

            await _callService.StartCallAsync(Friend.Id);
        }

        public async void StopCallAsync()
        {
            if (_timer.IsEnabled)
                _timer.Stop();

            await _restClient.SaveCallInfoAsync(Friend.Id, Duration);
            //await _callService.StopCallAsync();
        }
    }
}