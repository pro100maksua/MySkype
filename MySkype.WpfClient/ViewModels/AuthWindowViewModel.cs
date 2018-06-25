using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MySkype.WpfClient.Services;
using Nito.Mvvm;
using ReactiveUI;

namespace MySkype.WpfClient.ViewModels
{
    public class AuthWindowViewModel : ViewModelBase
    {
        public string Login { get; set; } = "Plotva";
        public string Password { get; set; } = "plotva";
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }


        private readonly RestSharpClient _restClient;
        private string _token;

        private ObservableCollection<string> _errorMessagges = new ObservableCollection<string>();
        private bool _isSignUp;

        public bool IsSignUp
        {
            get => _isSignUp;
            set => this.RaiseAndSetIfChanged(ref _isSignUp, value);
        }
        public ObservableCollection<string> ErrorMessages
        {
            get => _errorMessagges;
            set => this.RaiseAndSetIfChanged(ref _errorMessagges, value);
        }

        public AuthWindowViewModel()
        {
            _restClient = new RestSharpClient(_token);

            SubmitCommand = new AsyncCommand(SignUpAsync);
            SignUpCommand = new AsyncCommand(() =>
            {
                ErrorMessages = new ObservableCollection<string>();
                return Task.Run(() => IsSignUp = true);
            });
        }

        public AsyncCommand SubmitCommand { get; set; }
        public AsyncCommand SignUpCommand { get; set; }

        public async Task<string> SignInAsync()
        {
            _token = await _restClient.RequestTokenAsync(Login, Password);

            if (_token == null)
            {
                ErrorMessages.Add(" - Invalid login or password.");
            }

            return _token;
        }

        public async Task SignUpAsync()
        {
            ErrorMessages = new ObservableCollection<string>();
            if (IsValid())
            {

                var signUpRequest = new SignUpRequest
                {
                    Login = Login,
                    Password = Password,
                    Email = Email,
                    FirstName = FirstName,
                    LastName = LastName
                };

                var statusCode = await _restClient.SignUpAsync(signUpRequest);

                if (statusCode == HttpStatusCode.BadRequest)
                {
                    ErrorMessages.Add(" - Login already exists.");
                    return;
                }

                IsSignUp = false;
            }
        }

        private bool IsValid()
        {
            var errors = 0;
            if (string.IsNullOrWhiteSpace(Login) || Login.Length < 4)
            {
                ErrorMessages.Add(" - Login should not be less than four characters.");
                errors++;
            }
            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 4)
            {
                ErrorMessages.Add(" - Password should not be less than four characters.");
                errors++;
            }
            if (string.IsNullOrWhiteSpace(Email) || !IsValidEmailAddress(Email))
            {
                ErrorMessages.Add(" - Email is invalid.");
                errors++;
            }

            return errors == 0;
        }
        public bool IsValidEmailAddress(string email)
        {
            var regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");

            return regex.IsMatch(email);
        }
    }
}
