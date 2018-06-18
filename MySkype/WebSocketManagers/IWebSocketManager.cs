using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using MySkype.Server.Models;

namespace MySkype.Server.WebSocketManagers
{
    public interface IWebSocketManager
    {
        void Add(Guid id, WebSocket socket);
        Task ReceiveBytesAsync(Guid senderSocketId, WebSocketReceiveResult result, byte[] buffer);
        Task ReceiveTextAsync(WebSocketReceiveResult result, byte[] buffer);
        Task RemoveAsync(Guid id);
        Task SendAsync(Guid senderId, Guid targetId, MessageType messageType);
    }
}