using System.Threading.Tasks;
using MySkype.WpfClient.Services;
using ReactiveUI;

namespace MySkype.WpfClient.ViewModels
{
    public class AuthWindowViewModel : ViewModelBase
    {
        public string Login { get; set; } = "Plotva";
        public string Password { get; set; } = "plotva";

        private readonly RestSharpClient _restClient;
        private string _token;

        private string _errorMessagge;
        public string ErrorMessage
        {
            get => _errorMessagge;
            set => this.RaiseAndSetIfChanged(ref _errorMessagge, value);
        }

        public AuthWindowViewModel()
        {
            _restClient = new RestSharpClient(_token);
        }

        public async Task<string> SignInAsync()
        {
            _token = await _restClient.RequestTokenAsync(Login, Password);

            if (_token == null)
            {
                ErrorMessage = "Invalid login or password.";
            }

            return _token;

        }
    }
}
