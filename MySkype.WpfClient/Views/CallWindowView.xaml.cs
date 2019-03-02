using System.Collections.Generic;
using MySkype.WpfClient.Models;
using MySkype.WpfClient.Services;
using MySkype.WpfClient.ViewModels;

namespace MySkype.WpfClient.Views
{
    public partial class CallWindowView
    {
        public CallWindowView(User user,List<User> friends, User friend, WebSocketClient webSocketClient, string token, NotificationService notificationService, bool isCaller)
        {
            InitializeComponent();

            var viewModel = new CallWindowViewModel(user,friends, friend, webSocketClient, token, notificationService, isCaller);
            DataContext = viewModel;

            viewModel.CloseRequested += (sender, e) => Close();
        }

        private void Friends_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
