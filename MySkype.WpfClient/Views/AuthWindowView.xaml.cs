using MySkype.WpfClient.ViewModels;

namespace MySkype.WpfClient.Views
{
    public partial class AuthWindowView
    {
        public string Token { get; set; }

        public AuthWindowView()
        {
            InitializeComponent();

            var viewModel = new AuthWindowViewModel();
            DataContext = viewModel;

            viewModel.CloseRequested += (sender, e) =>
            {
                Token = e.Token;
                Close();
            };
        }
    }
}
