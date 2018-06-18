using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MySkype.Client.Models;
using MySkype.Client.Services;
using MySkype.Client.ViewModels;

namespace MySkype.Client.Views
{
    public class CallRequestView : Window
    {
        private readonly User _caller;
        private RestSharpClient _restClient;
        private Button _rejectCallButton;
        private Button _acceptAudioCallButton;
        private Button _acceptVideoCallButton;

        public CallRequestView(User caller, RestSharpClient restClient)
        {
            _caller = caller;
            _restClient = restClient;

            DataContext = caller;

            InitializeComponent();
            this.AttachDevTools();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoaderPortableXaml.Load(this);

            _rejectCallButton = this.Find<Button>("RejectCallButton");
            _acceptAudioCallButton = this.Find<Button>("AcceptAudioCallButton");
            _acceptVideoCallButton = this.Find<Button>("AcceptVideoCallButton");

            _rejectCallButton.Click += Close;
            _acceptAudioCallButton.Click += AcceptAudioCallAsync;
            _acceptVideoCallButton.Click += AcceptVideoCallAsync;
        }

        private async void AcceptVideoCallAsync(object sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private async void AcceptAudioCallAsync(object sender, RoutedEventArgs e)
        {
            await _restClient.ConfirmAudioCallAsync(_caller.Id);

            Close(true);
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
