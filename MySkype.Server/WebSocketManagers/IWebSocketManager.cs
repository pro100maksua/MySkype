using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using MySkype.Server.Models;

namespace MySkype.Server.WebSocketManagers
{
    public interface IWebSocketManager
    {
        void Add(Guid socketId, WebSocket socket);
        Task RemoveAsync(Guid socketId);
        Task SendAsync(MessageBase message);
        Task ReceiveAsync(Guid id, WebSocketReceiveResult result, byte[] buffer);
        Task<bool> CheckIfUserIsOnlineAsync(Guid id);
    }
}