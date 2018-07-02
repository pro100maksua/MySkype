using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySkype.Server.Models;
using Newtonsoft.Json;

namespace MySkype.Server.WebSocketManagers
{
    public class WebSocketVideoManager : IWebSocketManager
    {
        private readonly WebSocketConnectionManager _connectionManager;

        public WebSocketVideoManager(WebSocketConnectionManager connectionManager)
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

        public async Task SendAsync(MessageBase message)
        {
            var targetSocket = _connectionManager.GetSocket(message.TargetId);

            if (targetSocket != null)
            {
                var json = JsonConvert.SerializeObject(message);

                await targetSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json), 0, json.Length),
                    WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public async Task ReceiveAsync(Guid id, WebSocketReceiveResult result, byte[] buffer)
        {
            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

            var data = JsonConvert.DeserializeObject<Data>(json);
            data.SenderId = id;

            await SendAsync(data);
        }

        public Task<bool> CheckIfUserIsOnlineAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task SendBytesAsync(Guid targetId, byte[] data)
        {
            var targetSocket = _connectionManager.GetSocket(targetId);

            if (targetSocket != null)
            {
                await targetSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary,
                    true, CancellationToken.None);
            }
        }
    }
}
