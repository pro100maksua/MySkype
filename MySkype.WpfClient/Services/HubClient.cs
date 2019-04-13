using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using MySkype.WpfClient.Models;

namespace MySkype.WpfClient.Services
{
    public class HubClient
    {
        private readonly NotificationService _notificationService;
        private readonly HubConnection _connection;

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public event EventHandler<ChatMessageReceivedEventArgs> MessageReceived;

        public HubClient(NotificationService notificationService, string token)
        {
            _notificationService = notificationService;
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/general",
                    options => options.AccessTokenProvider = () => Task.FromResult(token))
                .AddMessagePackProtocol()
                .Build();

            _connection.On<Message>("ReceiveMessage", ReceiveMessage);
            _connection.On<Notification>("ReceiveNotification", ReceiveNotification);
            _connection.On<Data>("ReceiveData", ReceiveData);
        }

        public async Task StartAsync()
        {
            await _connection.StartAsync();
        }

        public async Task CloseConnectionAsync()
        {
            await _connection.StopAsync();
        }

        public async Task SendNotificationAsync(Guid targetId, NotificationType notificationType)
        {
            var notification = new Notification
            {
                TargetId = targetId,
                NotificationType = notificationType
            };

            await _connection.SendAsync("SendNotificationAsync", notification);
        }

        public async Task SendMessageAsync(string message)
        {
            await _connection.SendAsync("SendMessageAsync", message);
        }

        public async Task SendDataAsync(byte[] bytes)
        {
            await _connection.SendAsync("SendDataAsync", bytes);
        }

        private void ReceiveNotification(Notification notification)
        {
            switch (notification.NotificationType)
            {
                case NotificationType.FriendRequest:
                    _notificationService.NotifyFriendRequest(notification.SenderId);
                    break;
                case NotificationType.CallRequest:
                    _notificationService.NotifyCallRequest(notification.SenderId);
                    break;
                case NotificationType.CallConfirmed:
                    _notificationService.NotifyCallAccepted(notification.SenderId);
                    break;
                case NotificationType.CallRejected:
                    _notificationService.NotifyCallRejected(notification.SenderId);
                    break;
                case NotificationType.CallEnded:
                    _notificationService.NotifyCallEnded(notification.SenderId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ReceiveMessage(Message message)
        {
            MessageReceived?.Invoke(this, new ChatMessageReceivedEventArgs
            {
                Content = message.Content,
                SenderId = message.SenderId
            });
        }

        private void ReceiveData(Data data)
        {
            DataReceived?.Invoke(this, new DataReceivedEventArgs
            {
                Data = data.Bytes,
                SenderId = data.SenderId
            });
        }
    }
}