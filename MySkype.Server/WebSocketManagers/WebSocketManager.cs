using System;
using System.Collections.Generic;
using System.Linq;
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
            var message = JsonConvert.DeserializeObject<MessageBase>(json);

            if (message.MessageType == MessageType.Data)
            {
                var data = JsonConvert.DeserializeObject<Data>(json);
                data.SenderId = id;
                await SendBytesAsync(data);

                return;
            }

            if (message.MessageType == MessageType.Notification)
            {
                var notification = JsonConvert.DeserializeObject<Notification>(json);

                notification.SenderId = id;
                if (notification.NotificationType == NotificationType.CallConfirmed)
                {
                    var call = _connectionManager.GetCall(notification.TargetId);
                    if (call == null)
                    {
                        _connectionManager.StartCall(notification.TargetId);
                    }
                    _connectionManager.AddCallFriend(notification.TargetId, id);

                    call = _connectionManager.GetCall(notification.TargetId).Where(userId => userId != id).ToHashSet();

                    foreach (var userId in call)
                    {
                        notification.TargetId = userId;
                        await SendAsync(notification);
                    }
                }
                else if (notification.NotificationType == NotificationType.CallEnded)
                {
                    var call = _connectionManager.GetCall(notification.SenderId);
                    if (call != null)
                    {
                        foreach (var userId in call)
                        {
                            notification.TargetId = userId;
                            await SendAsync(notification);
                        }

                        _connectionManager.RemoveCall(call);
                    }
                }
                else
                {
                    await SendAsync(notification);
                }
            }
            else if (message.MessageType == MessageType.Message)
            {
                message = JsonConvert.DeserializeObject<Message>(json);
                message.SenderId = id;
                await SendMessageAsync(message);
            }
        }

        private async Task SendMessageAsync(MessageBase message)
        {
            var call = _connectionManager.GetCall(message.SenderId);
            
            foreach (var callPart in call)
            {
                var targetSocket = _connectionManager.GetSocket(callPart);
                var json = JsonConvert.SerializeObject(message);

                await targetSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json), 0, json.Length),
                    WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public async Task<bool> CheckIfUserIsOnlineAsync(Guid id)
        {
            return await Task.Run(() =>
            {
                var targetSocket = _connectionManager.GetSocket(id);

                return targetSocket != null && targetSocket.State == WebSocketState.Open;
            });
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

        public async Task SendBytesAsync(MessageBase message)
        {
            var call = _connectionManager.GetCall(message.SenderId);
            if (call != null)
            {
                foreach (var callPart in call.Where(c => c != message.SenderId))
                {
                    var targetSocket = _connectionManager.GetSocket(callPart);
                    var json = JsonConvert.SerializeObject(message);

                    await targetSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json), 0, json.Length),
                        WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        public IEnumerable<Guid> GetCallParticipants(Guid callId)
        {
            return _connectionManager.GetCall(callId);
        }
    }
}