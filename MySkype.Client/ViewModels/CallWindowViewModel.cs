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

        private DispatcherTimer _timer = new DispatcherTimer( );
        private TimeSpan _duration = TimeSpan.FromSeconds(0);
        
        public bool Started { get; set; }
        public User Friend { get; set; }
        public TimeSpan Duration
        {
            get => _duration;
            set => this.RaiseAndSetIfChanged(ref _duration, value);
        }
        
        public CallWindowViewModel(User friend, WebSocketClient webSocketClient, RestSharpClient restClient,bool started)
        {
            Friend = friend;
            Started = started;
            
            _restClient = restClient;
            _callService = new CallService(webSocketClient);

            if (Started == false)
            {
                StartCallAsync();
            }
        }

        public async Task StartCallAsync()
        {
            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal,
                delegate
                {
                    Duration = Duration.Add(TimeSpan.FromSeconds(1));
                });

            _timer.Start();

            //await _callService.StartCallAsync();
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