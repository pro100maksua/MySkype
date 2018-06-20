using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MySkype.Client.Models;
using MySkype.Client.Services;
using MySkype.Client.ViewModels;

namespace MySkype.Client.Views
{
    public class CallWindowView : Window
    {
        private CallWindowViewModel _viewModel;
        private Button _stopCallButton;

        public CallWindowView(User friend, WebSocketClient webSocketClient, RestSharpClient restClient, NotificationService notificationService, bool staarted)
        {
            _viewModel = new CallWindowViewModel(friend, webSocketClient, restClient, notificationService, staarted);
            DataContext = _viewModel;

            InitializeComponent();
            this.AttachDevTools();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoaderPortableXaml.Load(this);
            _stopCallButton = this.Find<Button>("StopCallButton");

            _stopCallButton.Click += (sender, args) =>
            {
                _viewModel.StopCallAsync();
                Close();
            };
        }
    }
}
