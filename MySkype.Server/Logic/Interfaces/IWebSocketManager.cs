using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using MySkype.Server.Data.Models;

namespace MySkype.Server.Logic.Interfaces
{
    public interface IWebSocketManager
    {
        void Add(Guid socketId, WebSocket socket);
        Task RemoveAsync(Guid socketId);
        Task SendAsync(MessageBase message);
        Task ReceiveAsync(Guid id, WebSocketReceiveResult result, byte[] buffer);
    }
}