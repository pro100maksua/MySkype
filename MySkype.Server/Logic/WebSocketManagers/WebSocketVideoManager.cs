using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySkype.Server.Data.Models;
using MySkype.Server.Logic.Interfaces;
using Newtonsoft.Json;

namespace MySkype.Server.Logic.WebSocketManagers
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
            _connectionManager.AddVideoSocket(id, socket);
        }

        public async Task RemoveAsync(Guid id)
        {
            await _connectionManager.RemoveVideoSocketAsync(id);
        }

        public async Task ReceiveAsync(Guid id, WebSocketReceiveResult result, byte[] buffer)
        {
            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

            var data = JsonConvert.DeserializeObject<Data.Models.Data>(json);
            data.SenderId = id;

            await SendAsync(data);
        }

        public async Task SendAsync(MessageBase message)
        {
            var call = _connectionManager.GetCall(message.SenderId);
            if (call != null)
            {
                foreach (var callPart in call.Where(c => c != message.SenderId))
                {
                    var targetSocket = _connectionManager.GetVideoSocket(callPart);
                    if (targetSocket != null)
                    {
                        var json = JsonConvert.SerializeObject(message);

                        await targetSocket.SendAsync(
                            new ArraySegment<byte>(Encoding.UTF8.GetBytes(json), 0, json.Length),
                            WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
        }

        public void SendBytes(Guid targetId, byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}