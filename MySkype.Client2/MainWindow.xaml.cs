using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using MySkype.Client2.Models;
using RestSharp;
using RestSharp.Extensions;

namespace MySkype.Client2
{
    public class MainWindow : Window
    {
        private readonly WebSocketClient _webSocketClient;
        private string _token;
        private Guid _userId;
        private ListBox _friends;
        private ContentControl _bigControl;

        public MainWindow()
        {
            _webSocketClient = new WebSocketClient(_token);

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();

#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoaderPortableXaml.Load(this);
            
            _friends = this.Find<ListBox>("friends");
            _bigControl = this.Find<ContentControl>("bigControl");
            
            Tapped += AuthenticateUser;
            _friends.Tapped += OnListItemTapped;
        }
       
        private async void AuthenticateUser(object sender, EventArgs e)
        {
            await Authenticate();

            _webSocketClient.StartAsync();

            Tapped -= AuthenticateUser;
        }

        private async Task Authenticate()
        {
            Hide();

            var authWindow = new AuthWindow();

            _token = await authWindow.ShowDialog<string>();

            var jwtToken = new JwtSecurityToken(_token);
            _userId = new Guid(jwtToken.Subject);

            _friends.Items = LoadRooms();

            Show();
        }

        private void OnListItemTapped(object sender, RoutedEventArgs e)
        {
            var item = e.Source.InteractiveParent as Control;
            var friend = item.DataContext;
        }

        private IEnumerable<User> LoadRooms()
        {
            var client = new RestClient("http://localhost:5000/");

            var request = new RestRequest("/api/users/{id}/friends", Method.GET);
            request.AddUrlSegment("id", _userId);
            request.AddHeader("Authorization", "Bearer " + _token);
            
            var friends = client.Execute<List<User>>(request).Data;

            foreach (var friend in friends)
            {
                request = new RestRequest("/api/photos/{friendId}/photo", Method.GET);
                request.AddUrlSegment("friendId", friend.Id.ToString());
                request.AddHeader("Authorization", "Bearer " + _token);

                var file = client.DownloadData(request);

                const string folder = "photos";
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var path = Path.Combine(folder, friend.Avatar.FileName);
                friend.Avatar.FileName = path;
                file.SaveAs(path);

                friend.Avatar.Bitmap = new Bitmap(path);
            }

            return friends;
        }
    }
}