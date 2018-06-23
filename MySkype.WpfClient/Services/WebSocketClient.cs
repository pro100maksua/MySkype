using System;
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

        public event EventHandler<DataReceivedEventArgs> DataReceived
        {
            add => _client.DataReceived += value;
            remove => _client.DataReceived -= value;
        }

        public WebSocketClient(NotificationService notificationService, string token)
        {
            _notificationService = notificationService;

            _client = new WebSocket("ws://localhost:5000", version: WebSocketVersion.Rfc6455,
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

        private async void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            await Task.Run(() =>
            {
                var message = JsonConvert.DeserializeObject<Message>(e.Message);

                switch (message.MessageType)
                {
                    case MessageType.FriendRequest:
                        _notificationService.NotifyFriendRequest(message.SenderId);
                        break;
                    case MessageType.CallRequest:
                        _notificationService.NotifyCallRequest(message.SenderId);
                        break;
                    case MessageType.CallConfirmation:
                        _notificationService.NotifyCallAccepted(message.SenderId);
                        break;
                }
            });
        }
    }
}