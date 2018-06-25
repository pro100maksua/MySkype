using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using MySkype.WpfClient.Models;
using MySkype.WpfClient.Services;
using MySkype.WpfClient.Views;
using Nito.Mvvm;
using ReactiveUI;
using RestSharp.Extensions;

namespace MySkype.WpfClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private RestSharpClient _restClient;
        private WebSocketClient _webSocketClient;
        private readonly NotificationService _notificationService;

        private ObservableCollection<User> _contacts = new ObservableCollection<User>();
        private ObservableCollection<User> _searchResult = new ObservableCollection<User>();
        private ObservableCollection<CallRepresentation> _calls = new ObservableCollection<CallRepresentation>();
        private ObservableCollection<Message> _notifications = new ObservableCollection<Message>();
        private User _user = new User();
        private LargeArea _large = new LargeArea();
        private bool _isLargeUserFriend;
        private string _searchQuery;
        private bool _isSearchBoxEmpty = true;
        private bool _isLargeUserSet;

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
        public LargeArea Large
        {
            get => _large;
            set
            {
                if (!IsLargeUserSet)
                    IsLargeUserSet = true;

                this.RaiseAndSetIfChanged(ref _large, value);
            }
        }
        public bool IsLargeUserSet
        {
            get => _isLargeUserSet;
            set => this.RaiseAndSetIfChanged(ref _isLargeUserSet, value);
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

            ChoosePhotoCommand = new AsyncCommand(ChoosePhotoAsync);
            AddFriendCommand = new AsyncCommand((senderId) => AddFriendAsync((Guid)senderId));
            SendFriendRequestCommand = new AsyncCommand(SendFriendRequestAsync);
            SendAudioCallRequestCommand = new AsyncCommand(SendAudioCallRequestAsync);
            //SendVideoCallRequestCommand = new AsyncCommand(SendVideoCallRequestAsync);

        }

        public AsyncCommand ChoosePhotoCommand { get; }
        public AsyncCommand AddFriendCommand { get; }
        public AsyncCommand SendFriendRequestCommand { get; }
        public AsyncCommand SendAudioCallRequestCommand { get; }
        public AsyncCommand SendVideoCallRequestCommand { get; }


        private async Task GetPhotoAsync(User user)
        {
            var file = await _restClient.GetPhotoAsync(user.Avatar.Id);

            const string folder = "photos";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var path = !user.Avatar.FileName.Contains(folder)
                ? Path.Combine(folder, user.Avatar.FileName)
                : user.Avatar.FileName;

            file.SaveAs(path);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(path, UriKind.Relative);
            bitmap.EndInit();

            user.Avatar.Bitmap = bitmap;
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
            var authWindow = new AuthWindowView();

            var token = string.Empty;
            authWindow.ShowDialog();
            token = authWindow.Token;

            _restClient = new RestSharpClient(token);
            _webSocketClient = new WebSocketClient(_notificationService, token);

            var jwtToken = new JwtSecurityToken(token);
            var userId = new Guid(jwtToken.Claims.FirstOrDefault(c => c.Type.Equals("sid"))?.Value);
            var user = await _restClient.GetUserAsync(userId);

            await GetPhotoAsync(user);
            User = user;

            await GetFriendsAsync();
            await GetUserCallsAsync();
            await GetFriendRequestsAsync();


            _webSocketClient.Start();
        }

        private async Task GetUserCallsAsync()
        {
            var calls = await _restClient.GetUserCallsAsync();
            
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
            }).OrderByDescending(c=>c.StartTime).ToList();

            Calls = new ObservableCollection<CallRepresentation>(callRepr);
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
            var dialog = new OpenFileDialog
            {
                Filter = "Image files (*.png)| *.png",
                Title = "Choose photo",
                Multiselect = false
            };
            if (dialog.ShowDialog() == true)
            {
                await _restClient.SetPhotoAsync(User, dialog.FileName);

                await GetPhotoAsync(User);
            }
        }


        public async Task SendFriendRequestAsync()
        {
            IsLargeUserFriend = true;

            await _restClient.SendFriendRequestAsync(Large.User.Id);
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

            await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                new Action(() => Notifications.Add(notification)));
        }

        private async void ReceiveFriendRequestAsync(object sender, MyEventArgs e)
        {
            await ReceiveFriendRequestAsync(e.SenderId);
        }

        public async void ReceiveCallAsync(object sender, MyEventArgs e)
        {
            var caller = Contacts.FirstOrDefault(c => c.Id == e.SenderId);

            await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                new Action(async () =>
                {
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
            var callView = new CallWindowView(User.Id, friend, _webSocketClient, _restClient, _notificationService, isCaller);

            callView.ShowDialog();
        }

        public void SetLargeArea(User user)
        {
            var large = new LargeArea
            {
                User = user,
                Calls = Calls.Where(c => c.UserId == user.Id).OrderBy(c=>c.StartTime).GroupBy(c=>c.StartTime.Date).ToList()
            };

            Large = large;

            IsLargeUserFriend = Contacts.Contains(user);
        }

        public async Task SendAudioCallRequestAsync()
        {
            _webSocketClient.SendMessage(Large.User.Id, MessageType.CallRequest);

            await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                new Action(async () => await ShowCallWindowAsync(Large.User, isCaller: true)));
        }
    }
}
