using MySkype.WpfClient.Models;
using MySkype.WpfClient.Services;
using MySkype.WpfClient.ViewModels;

namespace MySkype.WpfClient.Views
{
    public partial class CallRequestView
    {
        public CallRequestView(User caller, WebSocketClient webSocketClient)
        {
            InitializeComponent();

            var viewModel = new CallRequestViewModel(caller, webSocketClient);
            DataContext = viewModel;

            viewModel.CloseRequested += (sender, e) =>
            {
                DialogResult = e.CallAccepted;
                Close();
            };
        }
    }
}
