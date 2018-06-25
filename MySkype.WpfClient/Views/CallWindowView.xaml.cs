using System;
using MySkype.WpfClient.Models;
using MySkype.WpfClient.Services;
using MySkype.WpfClient.ViewModels;

namespace MySkype.WpfClient.Views
{
    public partial class CallWindowView
    {
        public CallWindowView(Guid userId, User friend, WebSocketClient webSocketClient, RestSharpClient restClient, NotificationService notificationService, bool isCaller)
        {
            InitializeComponent();

            var viewModel = new CallWindowViewModel(userId, friend, webSocketClient, restClient, notificationService, isCaller);
            DataContext = viewModel;

            viewModel.CloseRequested += (sender, e) =>
            {
                Close();
            };
        }
    }
}
