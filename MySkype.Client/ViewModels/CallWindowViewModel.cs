using System;
using Avalonia.Threading;
using MySkype.Client.Models;
using MySkype.Client.Services;
using ReactiveUI;

namespace MySkype.Client.ViewModels
{
    class CallWindowViewModel : ViewModelBase
    {
        private readonly WebSocketClient _webSocketClient;
        private readonly RestSharpClient _restClient;
        private CallService _callService;

        private DispatcherTimer _timer;
        private TimeSpan _duration = TimeSpan.FromSeconds(0);
        
        public User Caller { get; set; }
        public TimeSpan Duration
        {
            get => _duration;
            set => this.RaiseAndSetIfChanged(ref _duration, value);
        }

        public CallWindowViewModel(User caller, WebSocketClient webSocketClient, RestSharpClient restClient)
        {
            Caller = caller;
            _webSocketClient = webSocketClient;
            _restClient = restClient;
            _callService = new CallService(_webSocketClient);


            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal,
                delegate
                {
                    Duration = Duration.Add(TimeSpan.FromSeconds(1));
                });

            _timer.Start();
        }

        public async void FinishCall()
        {
            _timer.Stop();

            await _restClient.SaveCallInfoAsync(Caller.Id, Duration);
        }
    }
}