using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySkype.Client.Models;
using Newtonsoft.Json;

namespace MySkype.Client.Services
{
    public class WebSocketClient
    {
        private readonly NotificationService _notificationService;
        private readonly string _token;
        private readonly ClientWebSocket _client;

        public WebSocketClient(NotificationService notificationService, string token)
        {
            _notificationService = notificationService;
            _token = token;

            _client = new ClientWebSocket();
        }

        public async Task StartAsync()
        {
            _client.Options.SetRequestHeader("Authorization",
                "Bearer " + _token);

            await _client.ConnectAsync(new Uri("ws://localhost:5000/"), CancellationToken.None);
        }

        public async Task ReceiveAsync()
        {
            var buffer = new byte[4 * 1024];

            while (_client.State == WebSocketState.Open)
            {
                var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                switch (result.MessageType)
                {
                    case WebSocketMessageType.Text:
                        await ReceiveMessageAsync(buffer, result);
                        break;
                    case WebSocketMessageType.Binary:
                        await ReceiveBytesAsync(buffer, result);
                        break;
                    case WebSocketMessageType.Close:
                        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty,
                            CancellationToken.None);
                        break;
                }
            }
        }

        private async Task ReceiveBytesAsync(byte[] buffer, WebSocketReceiveResult result)
        {

        }

        private async Task ReceiveMessageAsync(byte[] buffer, WebSocketReceiveResult result)
        {
            await Task.Run(() =>
            {
                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                var message = JsonConvert.DeserializeObject<Message>(json);

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