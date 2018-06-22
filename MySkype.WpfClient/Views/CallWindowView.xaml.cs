using System.Windows;
using MySkype.WpfClient.Models;
using MySkype.WpfClient.Services;
using MySkype.WpfClient.ViewModels;

namespace MySkype.WpfClient.Views
{
    /// <summary>
    /// Interaction logic for CallWindowView.xaml
    /// </summary>
    public partial class CallWindowView : Window
    {
        private readonly CallWindowViewModel _viewModel;

        public CallWindowView(User friend, WebSocketClient webSocketClient, RestSharpClient restClient, NotificationService notificationService, bool staarted)
        {
            InitializeComponent();

            _viewModel = new CallWindowViewModel(friend, webSocketClient, restClient, notificationService, staarted);
            DataContext = _viewModel;

            StopCallButton.Click += (sender, args) =>
            {
                _viewModel.StopCallAsync();
                Close();
            };
        }

        
    }
}
