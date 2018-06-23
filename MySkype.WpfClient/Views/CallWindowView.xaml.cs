using MySkype.WpfClient.Models;
using MySkype.WpfClient.Services;
using MySkype.WpfClient.ViewModels;

namespace MySkype.WpfClient.Views
{
    public partial class CallWindowView
    {
        public CallWindowView(User friend, WebSocketClient webSocketClient, RestSharpClient restClient, NotificationService notificationService, bool started)
        {
            InitializeComponent();

            var viewModel = new CallWindowViewModel(friend, webSocketClient, restClient, notificationService, started);
            DataContext = viewModel;

            StopCallButton.Click += (sender, args) =>
            {
                viewModel.StopCall();
                Close();
            };
        }
    }
}
