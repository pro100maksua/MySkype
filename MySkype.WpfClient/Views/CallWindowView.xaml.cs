using MySkype.WpfClient.Models;
using MySkype.WpfClient.Services;
using MySkype.WpfClient.ViewModels;

namespace MySkype.WpfClient.Views
{
    public partial class CallWindowView
    {
        public CallWindowView(User user, User friend, WebSocketClient webSocketClient, string token, RestSharpClient restClient, NotificationService notificationService, bool isCaller)
        {
            InitializeComponent();

            var viewModel = new CallWindowViewModel(user, friend, webSocketClient, token, restClient, notificationService, isCaller);
            DataContext = viewModel;

            viewModel.CloseRequested += (sender, e) => Close();
        }
    }
}
