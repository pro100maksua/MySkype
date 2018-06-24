using System;
using MySkype.WpfClient.Models;
using MySkype.WpfClient.Services;
using MySkype.WpfClient.ViewModels;

namespace MySkype.WpfClient.Views
{
    public partial class CallWindowView
    {
        public CallWindowView(Guid userId, User friend, WebSocketClient webSocketClient, RestSharpClient restClient, NotificationService notificationService, bool started)
        {
            InitializeComponent();

            var viewModel = new CallWindowViewModel(userId, friend, webSocketClient, restClient, notificationService, started);
            DataContext = viewModel;

            viewModel.CloseRequested += (sender, e) =>
            {
                Close();
            };

            StopCallButton.Click += (sender, args) =>
            {
                viewModel.StopCall();
                Close();
            };
        }


    }
}
