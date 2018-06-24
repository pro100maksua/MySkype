using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MySkype.Server.WebSocketManagers
{
    public class WebSocketConnectionManager
    {
        private readonly ConcurrentDictionary<Guid, WebSocket> _sockets =
            new ConcurrentDictionary<Guid, WebSocket>();

        public ConcurrentDictionary<Guid, WebSocket> GetAll()
        {
            return _sockets;
        }

        public WebSocket Get(Guid id)
        {
            return _sockets.TryGetValue(id, out var socket) ? socket : null;
        }

        public void AddSocket(Guid id, WebSocket socket)
        {
            _sockets[id] = socket;
        }

        public async Task RemoveSocketAsync(Guid id)
        {
            _sockets.TryRemove(id, out var socket);

            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "normal closure", CancellationToken.None);
        }
    }
}
