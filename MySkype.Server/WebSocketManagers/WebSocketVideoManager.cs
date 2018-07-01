﻿using System;
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

        public Task SendMessageAsync(MessageBase message)
        {
            throw new NotImplementedException();
        }

        public async Task ReceiveAsync(Guid id, WebSocketReceiveResult result, byte[] buffer)
        {
            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

            var data = JsonConvert.DeserializeObject<Data>(json);

            await SendBytesAsync(data.TargetId, data.Bytes);
        }

        public Task<bool> CheckIfUserIsOnlineAsync(Guid id)
        {
            throw new NotImplementedException();
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
