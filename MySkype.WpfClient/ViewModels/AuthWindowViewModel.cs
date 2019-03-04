using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MySkype.WpfClient.ApiInterfaces;
using MySkype.WpfClient.Models;
using Nito.Mvvm;
using ReactiveUI;
using RestEase;

namespace MySkype.WpfClient.ViewModels
{
    public class AuthWindowViewModel : ViewModelBase
    {
        private readonly ILoginApi _loginClient;
        private readonly IUsersApi _userClient;

        private bool _isSignUp;
        private ObservableCollection<string> _errorMessages;
        private SignUpRequest _form = new SignUpRequest();

        public bool IsSignUp
        {
            get => _isSignUp;
            set => this.RaiseAndSetIfChanged(ref _isSignUp, value);
        }
        public ObservableCollection<string> ErrorMessages
        {
            get => _errorMessages;
            set => this.RaiseAndSetIfChanged(ref _errorMessages, value);
        }
        public SignUpRequest Form
        {
            get => _form;
            set => this.RaiseAndSetIfChanged(ref _form, value);
        }
        
        public AsyncCommand SubmitCommand { get; }
        public AsyncCommand SignUpCommand { get; }
        public AsyncCommand SignInCommand { get; }

        public AuthWindowViewModel()
        {
            const string url = "http://localhost:5000/api/";
            _loginClient = RestClient.For<ILoginApi>(url + "identity");
            _userClient = RestClient.For<IUsersApi>(url + "users");

            SubmitCommand = new AsyncCommand(SubmitAsync);
            SignInCommand = new AsyncCommand(SignInAsync);
            SignUpCommand = new AsyncCommand(() =>
            {
                ErrorMessages = new ObservableCollection<string>();
                Form = new SignUpRequest();
                return Task.Run(() => IsSignUp = true);
            });
        }


        public event EventHandler<AuthEventArgs> CloseRequested;

        private void OnCloseRequested(string token)
        {
            CloseRequested?.Invoke(this, new AuthEventArgs { Token = token });
        }

        public async Task SignInAsync()
        {
            ErrorMessages = new ObservableCollection<string>();

            if (string.IsNullOrWhiteSpace(Form.Login) || string.IsNullOrWhiteSpace(Form.Password))
            {
                ErrorMessages.Add(" - Fields can't be empty.");
                return;
            }
            var tokenRequest = new TokenRequest { Login = Form.Login, Password = Form.Password };
            var response = await _loginClient.LoginAsync(tokenRequest);

            if (!response.ResponseMessage.IsSuccessStatusCode)
            {
                ErrorMessages.Add(" - Invalid login or password.");
            }
            else
            {
                OnCloseRequested(response.StringContent);
            }
        }

        public async Task SubmitAsync()
        {
            ErrorMessages = new ObservableCollection<string>();
            if (IsValid())
            {
                var response = await _userClient.RegisterAsync(Form);

                if (response.ResponseMessage.StatusCode == HttpStatusCode.BadRequest)
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
}
