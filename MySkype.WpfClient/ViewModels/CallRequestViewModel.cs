using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MySkype.WpfClient.Models;
using MySkype.WpfClient.Services;
using Nito.Mvvm;

namespace MySkype.WpfClient.ViewModels
{
    class CallRequestViewModel : ViewModelBase
    {
        private readonly WebSocketClient _webSocketClient;

        public User Caller { get; }

        public AsyncCommand AcceptCallCommand { get; }
        public AsyncCommand RejectCallCommand { get; }

        public CallRequestViewModel(User caller, WebSocketClient webSocketClient)
        {
            Caller = caller;
            _webSocketClient = webSocketClient;

            AcceptCallCommand = new AsyncCommand(() => MakeDecision(true));
            RejectCallCommand = new AsyncCommand(() => MakeDecision(false));
        }

        public event EventHandler<CloseEventArgs> CloseRequested;

        public void OnCloseRequested(bool result)
        {
            CloseRequested?.Invoke(this, new CloseEventArgs { CallAccepted = result });
        }

        public async Task MakeDecision(bool callAccepted)
        {
            await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(async () =>
            {
                OnCloseRequested(callAccepted);

                var messageType = callAccepted ? NotificationType.CallConfirmed : NotificationType.CallRejected;

                await _webSocketClient.SendNotificationAsync(Caller.Id, messageType);
            }));
        }
    }
}
