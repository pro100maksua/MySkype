using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MySkype.WpfClient.Models;
using MySkype.WpfClient.Services;
using Nito.Mvvm;
using ReactiveUI;

namespace MySkype.WpfClient.ViewModels
{
    public class AuthWindowViewModel : ViewModelBase
    {
        private readonly RestSharpClient _restClient;
        private ObservableCollection<string> _errorMessagges;
        private string _token;
        private bool _isSignUp;
        private SignUpRequest _form = new SignUpRequest();

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
        public SignUpRequest Form
        {
            get => _form;
            set => this.RaiseAndSetIfChanged(ref _form, value);
        }

        public AuthWindowViewModel()
        {
            _restClient = new RestSharpClient(_token);

            SubmitCommand = new AsyncCommand(SubmitAsync);
            SignInCommand = new AsyncCommand(SignInAsync);
            SignUpCommand = new AsyncCommand(() =>
            {
                ErrorMessages = new ObservableCollection<string>();
                Form = new SignUpRequest();
                return Task.Run(() => IsSignUp = true);
            });
        }

        public AsyncCommand SubmitCommand { get; }
        public AsyncCommand SignUpCommand { get; }
        public AsyncCommand SignInCommand { get; }

        public event EventHandler<AuthEventArgs> CloseRequested;

        private void OnCloseRequested()
        {
            CloseRequested?.Invoke(this, new AuthEventArgs { Token = _token });
        }

        public async Task SignInAsync()
        {
            ErrorMessages = new ObservableCollection<string>();
            if (Form.Login != null && Form.Password != null)
            {
                var tokenRequest = new TokenRequest { Login = Form.Login, Password = Form.Password };

                _token = await _restClient.RequestTokenAsync(tokenRequest);

                if (_token == null)
                {
                    ErrorMessages.Add(" - Invalid login or password.");
                }
                else
                {
                    OnCloseRequested();
                }
            }
            else
            {
                ErrorMessages.Add(" - Fields can't be empty.");
            }
        }

        public async Task SubmitAsync()
        {
            ErrorMessages = new ObservableCollection<string>();
            if (IsValid())
            {
                var statusCode = await _restClient.SignUpAsync(Form);

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
            if (string.IsNullOrWhiteSpace(Form.Login) || Form.Login.Length < 4)
            {
                ErrorMessages.Add(" - Login should not be less than four characters.");
                errors++;
            }
            if (string.IsNullOrWhiteSpace(Form.Password) || Form.Password.Length < 4)
            {
                ErrorMessages.Add(" - Password should not be less than four characters.");
                errors++;
            }
            if (string.IsNullOrWhiteSpace(Form.Email) || !IsValidEmail(Form.Email))
            {
                ErrorMessages.Add(" - Email is invalid.");
                errors++;
            }

            if (string.IsNullOrWhiteSpace(Form.FirstName))
            {
                ErrorMessages.Add(" - First name cannot be empty.");
                errors++;
            }

            if (string.IsNullOrWhiteSpace(Form.LastName))
            {
                ErrorMessages.Add(" - Last name cannot be empty.");
                errors++;
            }

            return errors == 0;
        }

        public bool IsValidEmail(string email)
        {
            var regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");

            return regex.IsMatch(email);
        }
    }

    public class AuthEventArgs : EventArgs
    {
        public string Token { get; set; }
    }
}
