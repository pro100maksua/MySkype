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
                var json = e.Message;

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


        //public async Task ReceiveAsync()
        //{
        //    var buffer = new byte[4 * 1024];

        //    while (_client.State == WebSocketState.Open)
        //    {
        //        var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        //        switch (result.MessageType)
        //        {
        //            case WebSocketMessageType.Text:
        //                await ReceiveMessageAsync(buffer, result);
        //                break;
        //            case WebSocketMessageType.Binary:
        //                await ReceiveBytesAsync(buffer, result);
        //                break;
        //            case WebSocketMessageType.Close:
        //                await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty,
        //                    CancellationToken.None);
        //                break;
        //        }
        //    }
        //}

        //private async Task ReceiveBytesAsync(byte[] buffer, WebSocketReceiveResult result)
        //{

        //}

        //private async Task ReceiveMessageAsync(byte[] buffer, WebSocketReceiveResult result)
        //{
        //    await Task.Run(() =>
        //    {
        //        var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

        //        var message = JsonConvert.DeserializeObject<Message>(json);

        //        switch (message.MessageType)
        //        {
        //            case MessageType.FriendRequest:
        //                _notificationService.NotifyFriendRequest(message.SenderId);
        //                break;
        //            case MessageType.CallRequest:
        //                _notificationService.NotifyCallRequest(message.SenderId);
        //                break;
        //            case MessageType.CallConfirmation:
        //                _notificationService.NotifyCallAccepted(message.SenderId);
        //                break;
        //        }
        //    });
        //}
    }
}