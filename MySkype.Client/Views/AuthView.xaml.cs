using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MySkype.Client.ViewModels;

namespace MySkype.Client.Views
{
    public class AuthView : Window
    {
        private Button _signInButton;
        private readonly AuthWindowViewModel _viewModel;

        public AuthView()
        {
            _viewModel = new AuthWindowViewModel();
            DataContext = _viewModel;

            InitializeComponent();
            this.AttachDevTools();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoaderPortableXaml.Load(this);

            _signInButton = this.Find<Button>("SignInButton");
            _signInButton.Click += OnSignInButtonClick;

        }

        private async void OnSignInButtonClick(object sender, RoutedEventArgs e)
        {
            var token = await _viewModel.SignInAsync();

            if (token != null)
            {
                Close(token);
            }
        }
    }
}
