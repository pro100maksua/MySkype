using System.Windows;
using MySkype.WpfClient.Models;
using MySkype.WpfClient.Services;

namespace MySkype.WpfClient.Views
{
    /// <summary>
    /// Interaction logic for CallRequestView.xaml
    /// </summary>
    public partial class CallRequestView : Window
    {
        public CallRequestView(User caller, RestSharpClient restClient)
        {
            InitializeComponent();


            _caller = caller;
            _restClient = restClient;

            DataContext = caller;

            RejectCallButton.Click += CloseWindow;
            AcceptAudioCallButton.Click += AcceptAudioCallAsync;
            AcceptVideoCallButton.Click += AcceptVideoCallAsync;
        }


        private readonly User _caller;
        private readonly RestSharpClient _restClient;
        

        private async void AcceptVideoCallAsync(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
            // Close(true);
        }

        private async void AcceptAudioCallAsync(object sender, RoutedEventArgs e)
        {
            await _restClient.ConfirmAudioCallAsync(_caller.Id);

            DialogResult = true;
            Close();
            // Close(true);
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
            //  Close(false);
        }
    }
}
