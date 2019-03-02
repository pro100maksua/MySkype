using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using MySkype.WpfClient.ApiInterfaces;
using MySkype.WpfClient.Models;
using MySkype.WpfClient.Services;
using MySkype.WpfClient.Views;
using Nito.Mvvm;
using ReactiveUI;
using RestEase;

namespace MySkype.WpfClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IUsersApi _usersClient;
        private readonly ICallsApi _callsClient;
        private readonly IPhotosApi _photosClient;
        private readonly IUserFriendsApi _userFriendsClient;
        private WebSocketClient _webSocketClient;
        private readonly NotificationService _notificationService;

        private bool _isLargeUserFriend;
        private bool _isSearchBoxEmpty = true;
        private bool _isFriendSet;
        private string _searchQuery;
        private User _friend;
        private User _user = new User();
        private ObservableCollection<User> _contacts = new ObservableCollection<User>();
        private ObservableCollection<User> _searchResult = new ObservableCollection<User>();
        private ObservableCollection<CallRepresentation> _calls = new ObservableCollection<CallRepresentation>();
        private ObservableCollection<Message> _notifications = new ObservableCollection<Message>();
        private List<IGrouping<DateTime, CallRepresentation>> _friendCalls =
            new List<IGrouping<DateTime, CallRepresentation>>();

        private string _token;

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
        public ObservableCollection<CallRepresentation> Calls
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
        public User Friend
        {
            get => _friend;
            set
            {
                if (!IsFriendSet)
                    IsFriendSet = true;

                this.RaiseAndSetIfChanged(ref _friend, value);
            }
        }
        public List<IGrouping<DateTime, CallRepresentation>> FriendCalls
        {
            get => _friendCalls;
            set => this.RaiseAndSetIfChanged(ref _friendCalls, value);

        }
        public bool IsFriendSet
        {
            get => _isFriendSet;
            set => this.RaiseAndSetIfChanged(ref _isFriendSet, value);
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

        public AsyncCommand InitCommand { get; set; }
        public AsyncCommand SearchCommand { get; set; }
        public AsyncCommand SetFriendCommand { get; set; }
        public AsyncCommand ChoosePhotoCommand { get; }
        public AsyncCommand AddFriendCommand { get; }
        public AsyncCommand SendFriendRequestCommand { get; }
        public AsyncCommand SendAudioCallRequestCommand { get; }
        public AsyncCommand SendVideoCallRequestCommand { get; }

        public MainWindowViewModel()
        {
            var authWindow = new AuthWindowView();
            authWindow.ShowDialog();
            _token = authWindow.Token;

            _usersClient = RestClient.For<IUsersApi>("http://localhost:5000/api/users");
            _userFriendsClient = RestClient.For<IUserFriendsApi>("http://localhost:5000/api/user/friends"); ;
            _callsClient = RestClient.For<ICallsApi>("http://localhost:5000/api/calls");
            _photosClient = RestClient.For<IPhotosApi>("http://localhost:5000/api/photos");

            var header = "Bearer " + _token;
            _usersClient.Token = header;
            _userFriendsClient.Token = header;
            _callsClient.Token = header;
            _photosClient.Token = header;

            _notificationService = new NotificationService();
            _notificationService.FriendRequestReceived += ReceiveFriendRequestAsync;
            _notificationService.CallRequestReceived += ReceiveCallAsync;

            SearchCommand = new AsyncCommand(SearchAsync);
            ChoosePhotoCommand = new AsyncCommand(ChoosePhotoAsync);
            SendFriendRequestCommand = new AsyncCommand(SendFriendRequestAsync);
            SendAudioCallRequestCommand = new AsyncCommand(SendAudioCallRequestAsync);
            AddFriendCommand = new AsyncCommand(senderId => AddFriendAsync((Guid)senderId));
            SetFriendCommand = new AsyncCommand(friend => SetLargeAreaAsync((User)friend));
            InitCommand = new AsyncCommand(InitAsync);
        }

        private async Task GetPhotoAsync(User user)
        {
            var file = await _photosClient.DownloadAsync(user.Avatar.Id);
           
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = file;
            bitmap.EndInit();

            user.Avatar.Bitmap = bitmap;
        }

        public async Task GetFriendsAsync()
        {
            var friends = await _userFriendsClient.GetFriendsAsync();

            foreach (var friend in friends)
            {
                await GetPhotoAsync(friend);
            }

            Contacts = new ObservableCollection<User>(friends);
        }

        public async Task InitAsync()
        {
            _webSocketClient = new WebSocketClient(_notificationService, _token, "general");

            var jwtToken = new JwtSecurityToken(_token);
            var userId = new Guid(jwtToken.Claims.FirstOrDefault(c => c.Type.Equals("sid"))?.Value);
            var user = await _usersClient.GetUserAsync(userId);

            await GetPhotoAsync(user);
            User = user;

            await GetFriendsAsync();
            await GetUserCallsAsync();
            await GetFriendRequestsAsync();

            _webSocketClient.Start();
        }

        private async Task GetUserCallsAsync()
        {
            var calls = await _callsClient.GetCallsAsync();

            var callRepr = calls.Select(c =>
            {
                var friendId = c.ParticipantIds.FirstOrDefault(p => p != User.Id);
                var friend = Contacts.FirstOrDefault(f => f.Id == friendId);

                return new CallRepresentation
                {
                    UserId = friendId,
                    UserFullName = friend.FullName,
                    UserAvatar = friend.Avatar.Bitmap,
                    Duration = new TimeSpan(c.Duration),
                    StartTime = new DateTime(c.StartTime)
                };
            }).OrderByDescending(c => c.StartTime).ToList();

            Calls = new ObservableCollection<CallRepresentation>(callRepr);
        }

        public async Task SearchAsync()
        {
            IEnumerable<User> users;
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                users = await _usersClient.GetAllAsync(SearchQuery);
            }
            else
            {
                users = await _userFriendsClient.GetFriendsAsync();
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

            var isAdded = await _userFriendsClient.ConfirmFriendRequestAsync(senderId);

            if (isAdded)
            {
                var friend = await _usersClient.GetUserAsync(senderId);
                await GetPhotoAsync(friend);

                Contacts.Add(friend);
            }
        }

        public async Task ChoosePhotoAsync()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image files (*.png)| *.png",
                Title = "Choose photo",
                Multiselect = false
            };
            if (dialog.ShowDialog() == true)
            {
                using (var content = new MultipartFormDataContent())
                {
                    var file = File.OpenRead(dialog.FileName);
                    content.Add(new StreamContent(file)
                    {
                        Headers =
                        {
                            ContentLength = file.Length,
                            ContentType = new MediaTypeHeaderValue("image/png")
                        }
                    }, "file", dialog.FileName);
                    await _photosClient.UploadAsync(User.Id, content);
                }

                await GetPhotoAsync(User);
            }
        }

        public async Task SendFriendRequestAsync()
        {
            IsLargeUserFriend = true;

            await _userFriendsClient.SendFriendRequestAsync(Friend.Id);
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
            var user = await _usersClient.GetUserAsync(senderId);
            var notification = new Message { SenderId = user.Id, SenderName = user.FullName };

            if (Notifications.FirstOrDefault(n => n.SenderId == notification.SenderId) != null) return;

            await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                new Action(() => Notifications.Add(notification)));
        }

        private async void ReceiveFriendRequestAsync(object sender, MyEventArgs e)
        {
            await ReceiveFriendRequestAsync(e.SenderId);
        }

        public async void ReceiveCallAsync(object sender, MyEventArgs e)
        {
            await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(async () =>
            {
                var caller = Contacts.FirstOrDefault(c => c.Id == e.SenderId);

                var callRequestWindow = new CallRequestView(caller, _webSocketClient);

                var accepted = callRequestWindow.ShowDialog();

                if (accepted.Value)
                {
                    await ShowCallWindowAsync(caller, isCaller: false);
                }
            }));
        }

        private async Task ShowCallWindowAsync(User friend, bool isCaller)
        {
            await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                var callView = new CallWindowView(User, Contacts.ToList(), friend, _webSocketClient, _token, _notificationService,
                    isCaller);

                callView.ShowDialog();
            }));
        }

        public async Task SetLargeAreaAsync(User user)
        {
            await Task.Run(() =>
            {
                if (user == null)
                {
                    return;
                }

                Friend = user;
                FriendCalls = Calls
                    .Where(c => c.UserId == user.Id)
                    .OrderBy(c => c.StartTime)
                    .GroupBy(c => c.StartTime.Date)
                    .ToList();

                IsLargeUserFriend = Contacts.Contains(user);
            });
        }

        public async Task SendAudioCallRequestAsync()
        {
            var isOnline = await _usersClient.CheckIfUserOnlineAsync(Friend.Id);

            if (!isOnline)
            {
                MessageBox.Show("User is offline");
                return;
            }

            _webSocketClient.SendNotificationAsync(Friend.Id, NotificationType.CallRequest);

            await ShowCallWindowAsync(Friend, isCaller: true);
        }
    }
}
