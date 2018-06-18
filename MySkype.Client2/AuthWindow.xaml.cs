using System.Net;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Newtonsoft.Json;
using RestSharp;

namespace MySkype.Client2
{
    public class AuthWindow : Window
    {
        private Image _image;
        private TextBox _login;
        private TextBox _password;
        private Button _signInButton;

        public AuthWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            _image = this.Find<Image>("authImage");
            _login = this.Find<TextBox>("login");
            _password = this.Find<TextBox>("password");
            _signInButton = this.Find<Button>("signInButton");

            _image.Source = new Bitmap("photos\\auth.png");
            _signInButton.Click += OnSignInButtonClick;

        }

        private void OnSignInButtonClick(object sender, RoutedEventArgs e)
        {
            var client = new RestClient("http://localhost:5000/");

            var tokenRequest = new TokenRequest { Login = _login.Text, Password = _password.Text };

            var request = new RestRequest("/api/identity", Method.POST);
            request.AddJsonBody(tokenRequest);

            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var definition = new {Token = string.Empty};
                var token = JsonConvert.DeserializeAnonymousType(response.Content,definition);

                Close(token.Token);
            }
        }
    }
    
    public class TokenRequest
    {
        public string Login { get; set; }

        public string Password { get; set; }
    }
}
