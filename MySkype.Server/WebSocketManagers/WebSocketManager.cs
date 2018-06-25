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


        public async Task ReceiveAsync(Guid id, WebSocketReceiveResult result, byte[] buffer)
        {
            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

            var message = JsonConvert.DeserializeObject<Message>(json);
            message.SenderId = id;

            if (!CheckIfUserOnline(message.TargetId))
            {
                message.MessageType = MessageType.UserOffline;
                message.TargetId = id;
            }

            await SendMessageAsync(message);
        }

        public bool CheckIfUserOnline(Guid id)
        {
            var targetSocket = _connectionManager.Get(id);

            return targetSocket != null;
        }

        public async Task SendMessageAsync(Message message)
        {
            var socket = _connectionManager.Get(message.TargetId);
            
            var json = JsonConvert.SerializeObject(message);

            await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json), 0, json.Length),
                WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task SendBytesAsync(Guid targetId, byte[] data)
        {
            var targetSocket = _connectionManager.Get(targetId);

            if (targetSocket != null)
            {
                await targetSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary,
                    true, CancellationToken.None);
            }
        }
    }
}
