using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MySkype.Client.Models;
using MySkype.Client.Services;
using MySkype.Client.Views;
using ReactiveUI;
using RestSharp.Extensions;

namespace MySkype.Client.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private RestSharpClient _restClient;
        private WebSocketClient _webSocketClient;
        private readonly NotificationService _notificationService;

        private ObservableCollection<User> _contacts = new ObservableCollection<User>();
        private ObservableCollection<User> _searchResult = new ObservableCollection<User>();
        private ObservableCollection<Call> _calls = new ObservableCollection<Call>();
        private ObservableCollection<Message> _notifications = new ObservableCollection<Message>();
        private User _user = new User();
        private User _largeUser = new User();
        private bool _isLargeUserFriend;
        private string _searchQuery;
        private bool _isSearchBoxEmpty = true;

        public ObservableCollection<User> Contacts
        {
            get => _contacts;
            set => this.RaiseAndSetIfChanged(ref _contacts, value);
        }
        public ObservableCollection<User> SearchResult
        {
            get => _searchResult;
            set => this.RaiseAndSetIfChanged(ref _searchResult, value);
        }
        public ObservableCollection<Call> Calls
        {
            get => _calls;
            set => this.RaiseAndSetIfChanged(ref _calls, value);
        }
        public ObservableCollection<Message> Notifications
        {
            get => _notifications;
            set => this.RaiseAndSetIfChanged(ref _notifications, value);
        }
        public User User
        {
            get => _user;
            set => this.RaiseAndSetIfChanged(ref _user, value);
        }
        public User LargeUser
        {
            get => _largeUser;
            set => this.RaiseAndSetIfChanged(ref _largeUser, value);
        }
        public bool IsLargeUserFriend
        {
            get => _isLargeUserFriend;
            set => this.RaiseAndSetIfChanged(ref _isLargeUserFriend, value);
        }
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                this.RaiseAndSetIfChanged(ref _searchQuery, value);
                if (string.IsNullOrWhiteSpace(value))
                {
                    IsSearchBoxEmpty = true;
                    SearchResult = null;
                }
                else
                {
                    IsSearchBoxEmpty = false;
                }
            }
        }
        public bool IsSearchBoxEmpty
        {
            get => _isSearchBoxEmpty;
            set => this.RaiseAndSetIfChanged(ref _isSearchBoxEmpty, value);
        }

        public MainWindowViewModel()
        {
            _notificationService = new NotificationService();

            _notificationService.FriendRequestReceived += ReceiveFriendRequestAsync;
            _notificationService.CallRequestReceived += ReceiveCallAsync;
            _notificationService.CallAccepted += StartCallAsync;
        }

        private async Task GetPhotoAsync(User user)
        {
            var file = await _restClient.GetPhotoAsync(user.Id);
            string path = string.Empty;

            const string folder = "photos";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            if (!user.Avatar.FileName.Contains(folder))
            {
                path = Path.Combine(folder, user.Avatar.FileName);
            }
            else
            {
                path = user.Avatar.FileName;
            }
            user.Avatar.FileName = path;
            file.SaveAs(path);

            user.Avatar.Bitmap = new Bitmap(path);
        }

        public async Task GetFriendsAsync()
        {
            var friends = await _restClient.GetFriendsAsync();

            foreach (var friend in friends)
            {
                await GetPhotoAsync(friend);
            }

            Contacts = new ObservableCollection<User>(friends);
        }

        public async Task InitAsync()
        {
            var authWindow = new AuthView();

            var token = await authWindow.ShowDialog<string>();

            _restClient = new RestSharpClient(token);
            _webSocketClient = new WebSocketClient(_notificationService, token);

            var jwtToken = new JwtSecurityToken(token);
            var userId = new Guid(jwtToken.Claims.FirstOrDefault(c => c.Type.Equals("sid"))?.Value);
            var user = await _restClient.GetUserAsync(userId);

            await GetPhotoAsync(user);
            User = user;

            await GetFriendsAsync();
            await GetFriendRequestsAsync();

            await _webSocketClient.StartAsync();
            _webSocketClient.ReceiveAsync();

        }

        public async Task SearchAsync()
        {
            List<User> users;
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                users = await _restClient.GetUsersAsync(SearchQuery);
            }
            else
            {
                users = await _restClient.GetFriendsAsync();
            }

            foreach (var user in users)
            {
                await GetPhotoAsync(user);
            }

            SearchResult = new ObservableCollection<User>(users);
        }

        public async Task AddFriendAsync(Guid senderId)
        {
            var notification = Notifications.FirstOrDefault(n => n.SenderId == senderId);
            Notifications.Remove(notification);

            var isAdded = await _restClient.AddFriendAsync(senderId);

            if (isAdded)
            {
                var friend = await _restClient.GetUserAsync(senderId);
                await GetPhotoAsync(friend);

                Contacts.Add(friend);
            }
        }

        public async Task ChoosePhotoAsync()
        {
            var filter = new FileDialogFilter { Extensions = new List<string> { "png" } };
            var dialog = new OpenFileDialog
            {
                Title = "Choose photo",
                AllowMultiple = false,
                Filters = new List<FileDialogFilter> { filter }
            };

            var fileNames = await dialog.ShowAsync();
            if (fileNames != null)
            {
                var photo = await _restClient.SetPhotoAsync(User, fileNames.FirstOrDefault());

                await GetPhotoAsync(User);

                //this.RaisePropertyChanged("User");
            }
        }


        public async Task SendFriendRequestAsync()
        {
            IsLargeUserFriend = true;

            await _restClient.SendFriendRequestAsync(LargeUser.Id);
        }

        private async Task GetFriendRequestsAsync()
        {
            foreach (var request in User.FriendRequests)
            {
                await ReceiveFriendRequestAsync(request);
            }
        }

        private async Task ReceiveFriendRequestAsync(Guid senderId)
        {
            var user = await _restClient.GetUserAsync(senderId);
            var notification = new Message { SenderId = user.Id, SenderName = user.FullName };

            if (Notifications.FirstOrDefault(n => n.SenderId == notification.SenderId) != null) return;

            await Dispatcher.UIThread.InvokeAsync(() => Notifications.Add(notification));
        }

        private async void ReceiveFriendRequestAsync(object sender, MyEventArgs e)
        {
            await ReceiveFriendRequestAsync(e.SenderId);
        }

        public async void ReceiveCallAsync(object sender, MyEventArgs e)
        {
            var caller = Contacts.FirstOrDefault(c => c.Id == e.SenderId);
            bool accepted;
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var callRequestWindow = new CallRequestView(caller, _restClient);

                accepted = await callRequestWindow.ShowDialog<bool>();

                if (accepted)
                {
                    await ShowCallWindowAsync(caller, false);
                }
            });


        }

        private async Task ShowCallWindowAsync(User friend, bool sentRequest)
        {
            var callView = new CallWindowView(friend, _webSocketClient, _restClient, sentRequest);

            await callView.ShowDialog();
        }

        public void SetLargeUser(User user)
        {
            LargeUser = user;

            var possibleFriend = Contacts.FirstOrDefault(c => c.Id == user.Id);

            IsLargeUserFriend = possibleFriend != null;
        }

        private async void StartCallAsync(object sender, MyEventArgs e)
        {
            var caller = await _restClient.GetUserAsync(e.SenderId);
            await GetPhotoAsync(caller);

            await ShowCallWindowAsync(caller, false);
        }

        public async Task SendVideoCallRequestAsync()
        {

        }

        public async Task SendAudioCallRequestAsync()
        {
            await _restClient.SendAudioCallRequestAsync(LargeUser.Id);

            await ShowCallWindowAsync(LargeUser, true);
        }
    }
}
