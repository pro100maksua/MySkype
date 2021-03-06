﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySkype.WpfClient.Models;
using Newtonsoft.Json;
using WebSocket4Net;

namespace MySkype.WpfClient.Services
{
    public class WebSocketClient
    {
        private readonly NotificationService _notificationService;
        private readonly WebSocket _client;

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public event EventHandler<ChatMessageReceivedEventArgs> MessageReceived;

        public WebSocketClient(NotificationService notificationService, string token, string route)
        {
            _notificationService = notificationService;

            _client = new WebSocket($"ws://localhost:5000/{route}", version: WebSocketVersion.Rfc6455,
                customHeaderItems: new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Authorization", "Bearer " + token)
                });

            _client.MessageReceived += OnMessageReceived;
        }

        public void Start()
        {
            _client.Open();
        }

        public void CloseConnection()
        {
            _client.Close("normal closure");
            _client.Dispose();
        }

        private async void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            await Task.Run(() =>
            {
                var messageBase = JsonConvert.DeserializeObject<MessageBase>(e.Message);

                switch (messageBase.MessageType)
                {
                    case MessageType.Notification:
                        var notification = JsonConvert.DeserializeObject<Notification>(e.Message);
                        HandleNotification(notification);
                        break;
                    case MessageType.Message:
                        var message = JsonConvert.DeserializeObject<Message>(e.Message);
                        HandleMessage(message);
                        break;
                    case MessageType.Data:
                        var data = JsonConvert.DeserializeObject<Data>(e.Message);
                        HandleData(data);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
        }

        public async Task SendNotificationAsync(Guid targetId, NotificationType notificationType)
        {
            await Task.Run(() =>
            {
                var notification = new Notification { TargetId = targetId, NotificationType = notificationType };

                var json = JsonConvert.SerializeObject(notification);

                _client.Send(json);
            });
        }

        public async Task SendMessageAsync(string content)
        {
            await Task.Run(() =>
            {
                var message = new Message {Content = content };

                var json = JsonConvert.SerializeObject(message);

                _client.Send(json);
            });
        }

        public async Task SendDataAsync(byte[] bytes)
        {
            await Task.Run(() =>
            {
                var data = new Data { Bytes = bytes };

                var json = JsonConvert.SerializeObject(data);

                _client.Send(json);
            });
        }

        private void HandleNotification(Notification notification)
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

        private void HandleMessage(Message message)
        {
            MessageReceived?.Invoke(this, new ChatMessageReceivedEventArgs
            {
                Content = message.Content,
                SenderId = message.SenderId
            });
        }

        private void HandleData(Data data)
        {
            DataReceived?.Invoke(this, new DataReceivedEventArgs
            {
                Data = data.Bytes,
                SenderId = data.SenderId
            });
        }
    }

    public class ChatMessageReceivedEventArgs : EventArgs
    {
        public string Content { get; set; }

        public Guid SenderId { get; set; }
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public Guid SenderId { get; set; }

        public byte[] Data { get; set; }
    }
}