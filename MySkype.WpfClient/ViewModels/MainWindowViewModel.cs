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
        private IUsersApi _usersClient;
        private ICallsApi _callsClient;
        private IPhotosApi _photosClient;
        private IUserFriendsApi _userFriendsClient;
        private NotificationService _notificationService;
        private WebSocketClient _webSocketClient;

        private string _token;
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

        public bool IsLargeUserFriend
        {
            get => _isLargeUserFriend;
            set => this.RaiseAndSetIfChanged(ref _isLargeUserFriend, value);
        }
        public bool IsSearchBoxEmpty
        {
            get => _isSearchBoxEmpty;
            set => this.RaiseAndSetIfChanged(ref _isSearchBoxEmpty, value);
        }
        public bool IsFriendSet
        {
            get => _isFriendSet;
            set => this.RaiseAndSetIfChanged(ref _isFriendSet, value);
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
        public User Friend
        {
            get => _friend;
            set
            {
                this.RaiseAndSetIfChanged(ref _friend, value);

                IsFriendSet = true;
            }
        }
        public User User
        {
            get => _user;
            set => this.RaiseAndSetIfChanged(ref _user, value);
        }
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
        public List<IGrouping<DateTime, CallRepresentation>> FriendCalls
        {
            get => _friendCalls;
            set => this.RaiseAndSetIfChanged(ref _friendCalls, value);

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

            InitCommand = new AsyncCommand(InitAsync);
            SearchCommand = new AsyncCommand(SearchAsync);
            ChoosePhotoCommand = new AsyncCommand(ChoosePhotoAsync);
            SendFriendRequestCommand = new AsyncCommand(SendFriendRequestAsync);
            SendAudioCallRequestCommand = new AsyncCommand(SendAudioCallRequestAsync);
            AddFriendCommand = new AsyncCommand(senderId => AddFriendAsync((Guid)senderId));
            SetFriendCommand = new AsyncCommand(friend => SetLargeAreaAsync((User)friend));
        }

        public async Task InitAsync()
        {
            const string url = "http://localhost:5000/api/";
            _usersClient = RestClient.For<IUsersApi>(url + "users");
            _userFriendsClient = RestClient.For<IUserFriendsApi>(url + "user/friends");
            _callsClient = RestClient.For<ICallsApi>(url + "calls");
            _photosClient = RestClient.For<IPhotosApi>(url + "photos");

            var header = "Bearer " + _token;
            _usersClient.Token = header;
            _userFriendsClient.Token = header;
            _callsClient.Token = header;
            _photosClient.Token = header;

            _notificationService = new NotificationService();
            _notificationService.FriendRequestReceived += ReceiveFriendRequestAsync;
            _notificationService.CallRequestReceived += ReceiveCallAsync;
            _webSocketClient = new WebSocketClient(_notificationService, _token, "general");

            var jwt = new JwtSecurityToken(_token);
            var userId = new Guid(jwt.Claims.First(c => c.Type.Equals("sid")).Value);

            var user = await _usersClient.GetUserAsync(userId);
            user.Avatar.Bitmap = await GetPhotoAsync(user.Avatar.Id);
            User = user;

            Contacts = new ObservableCollection<User>(await GetFriendsAsync());
            Calls = new ObservableCollection<CallRepresentation>(await GetUserCallsAsync());
            Notifications = new ObservableCollection<Message>(await GetFriendRequestsAsync());

            _webSocketClient.Start();
        }

        private async Task<BitmapImage> GetPhotoAsync(Guid photoId)
        {
            var photo = await _photosClient.DownloadAsync(photoId);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = photo;
            bitmap.EndInit();

            return bitmap;
        }

        public async Task<List<User>> GetFriendsAsync()
        {
            var friends = (await _userFriendsClient.GetFriendsAsync()).ToList();
            foreach (var friend in friends)
            {
                friend.Avatar.Bitmap = await GetPhotoAsync(friend.Avatar.Id);
            }

            return friends;
        }

        private async Task<List<CallRepresentation>> GetUserCallsAsync()
        {
            var calls = await _callsClient.GetCallsAsync();

            var callRepresentations = calls.Select(c =>
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

            return callRepresentations;
        }

        public async Task SearchAsync()
        {
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var users = (await _usersClient.GetAllAsync(SearchQuery)).ToList();
                foreach (var user in users)
                {
                    user.Avatar.Bitmap = await GetPhotoAsync(user.Avatar.Id);
                }

                SearchResult = new ObservableCollection<User>(users);
            }
        }

        public async Task AddFriendAsync(Guid senderId)
        {
            var notification = Notifications.FirstOrDefault(n => n.SenderId == senderId);
            Notifications.Remove(notification);

            var isAdded = await _userFriendsClient.ConfirmFriendRequestAsync(senderId);
            if (isAdded)
            {
                var friend = await _usersClient.GetUserAsync(senderId);
                friend.Avatar.Bitmap = await GetPhotoAsync(friend.Avatar.Id);

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

                User.Avatar.Bitmap = await GetPhotoAsync(User.Avatar.Id);
            }
        }

        public async Task SendFriendRequestAsync()
        {
            IsLargeUserFriend = true;

            await _userFriendsClient.SendFriendRequestAsync(Friend.Id);
        }

        private async Task<List<Message>> GetFriendRequestsAsync()
        {
            var notifications = new List<Message>();
            foreach (var userId in User.FriendRequests)
            {
                var user = await _usersClient.GetUserAsync(userId);
                var notification = new Message { SenderId = user.Id, SenderName = user.FullName };
                notifications.Add(notification);
            }

            return notifications;
        }

        private async Task ReceiveFriendRequestAsync(Guid senderId)
        {
            var user = await _usersClient.GetUserAsync(senderId);
            var notification = new Message { SenderId = user.Id, SenderName = user.FullName };

            await Application.Current.Dispatcher.InvokeAsync(() => Notifications.Add(notification));
        }

        private async void ReceiveFriendRequestAsync(object sender, MyEventArgs e)
        {
            await ReceiveFriendRequestAsync(e.SenderId);
        }

        public async void ReceiveCallAsync(object sender, MyEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                var caller = Contacts.FirstOrDefault(c => c.Id == e.SenderId);

                var callRequestWindow = new CallRequestView(caller, _webSocketClient);

                var accepted = callRequestWindow.ShowDialog();
                if (accepted.HasValue && accepted.Value)
                {
                    await ShowCallWindowAsync(caller, isCaller: false);
                }
            });
        }

        private async Task ShowCallWindowAsync(User friend, bool isCaller)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var callView = new CallWindowView(User, Contacts.ToList(), friend, _webSocketClient, _token, _notificationService,
                    isCaller);

                callView.ShowDialog();
            });
        }

        public async Task SetLargeAreaAsync(User user)
        {
            if (user == null)
            {
                return;
            }

            await Task.Run(() =>
            {
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

            await _webSocketClient.SendNotificationAsync(Friend.Id, NotificationType.CallRequest);

            await ShowCallWindowAsync(Friend, isCaller: true);
        }
    }
}