using System.Windows;
using MySkype.WpfClient.Models;
using MySkype.WpfClient.Services;

namespace MySkype.WpfClient.Views
{
    public partial class CallRequestView
    {

        private readonly User _caller;
        private readonly WebSocketClient _webSocketClient;

        public CallRequestView(User caller, WebSocketClient webSocketClient)
        {
            InitializeComponent();

            _caller = caller;
            _webSocketClient = webSocketClient;

            DataContext = caller;

            RejectCallButton.Click += CloseWindow;
            AcceptAudioCallButton.Click += AcceptCall;
            AcceptVideoCallButton.Click += AcceptCall;
        }

        private void AcceptCall(object sender, RoutedEventArgs e)
        {
            _webSocketClient.SendMessage(_caller.Id, MessageType.CallConfirmed);

            DialogResult = true;
            Close();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            _webSocketClient.SendMessage(_caller.Id, MessageType.CallRejected);

            DialogResult = false;
            Close();
        }
    }
}
