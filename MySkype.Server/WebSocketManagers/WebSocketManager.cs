using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySkype.Server.Models;
using Newtonsoft.Json;

namespace MySkype.Server.WebSocketManagers
{
    public class WebSocketManager : IWebSocketManager
    {
        private readonly WebSocketConnectionManager _connectionManager;

        public WebSocketManager(WebSocketConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public void Add(Guid id, WebSocket socket)
        {
            _connectionManager.AddSocket(id, socket);
        }

        public async Task RemoveAsync(Guid id)
        {
            await _connectionManager.RemoveSocketAsync(id);
        }

        public async Task ReceiveBytesAsync(Guid senderSocketId, WebSocketReceiveResult result, byte[] buffer)
        {
            await SendBytesAsync(senderSocketId, result, buffer);
        }
        public async Task SendAsync(Guid senderId, Guid targetId, MessageType messageType)
        {
            var targetSocket = _connectionManager.Get(targetId);

            if (targetSocket != null)
            {
                var message = new Message
                {
                    MessageType = messageType,
                    SenderId = senderId
                };

                var json = JsonConvert.SerializeObject(message);

                await targetSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json), 0, json.Length),
                    WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public async Task SendDataAsync(Guid senderId, Guid targetId, byte[] data)
        {
            var targetSocket = _connectionManager.Get(senderId);

            if (targetSocket != null)
            {
                await targetSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true,
                    CancellationToken.None);
            }
        }

        private async Task SendBytesAsync(Guid targetSocketId, WebSocketReceiveResult result, byte[] bytes)
        {
            var targetSocket = _connectionManager.Get(targetSocketId);

            if (targetSocket.State == WebSocketState.Open)
            {
                await targetSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Binary,
                    result.EndOfMessage,
                    CancellationToken.None);
            }
        }
    }
}
