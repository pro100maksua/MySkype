﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MySkype.Server.Logic.WebSocketManagers
{
    public class WebSocketConnectionManager
    {
        private readonly ConcurrentDictionary<Guid, WebSocket> _sockets =
            new ConcurrentDictionary<Guid, WebSocket>();

        private readonly ConcurrentDictionary<Guid, WebSocket> _videoSockets =
            new ConcurrentDictionary<Guid, WebSocket>();

        private readonly ConcurrentDictionary<Guid, HashSet<Guid>> _calls =
            new ConcurrentDictionary<Guid, HashSet<Guid>>();

        public WebSocket GetSocket(Guid id)
        {
            return _sockets.TryGetValue(id, out var socket) ? socket : null;
        }
        
        public WebSocket GetVideoSocket(Guid id)
        {
            return _videoSockets.TryGetValue(id, out var socket) ? socket : null;
        }

        public void AddSocket(Guid id, WebSocket socket)
        {
            _sockets[id] = socket;
        }

        public void AddVideoSocket(Guid id, WebSocket socket)
        {
            _videoSockets[id] = socket;
        }

        public async Task RemoveSocketAsync(Guid id)
        {
            _sockets.TryRemove(id, out var socket);
            if (socket != null)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "normal closure", CancellationToken.None);
            }
        }

        public async Task RemoveVideoSocketAsync(Guid id)
        {
            _videoSockets.TryRemove(id, out var socket);
            if (socket != null)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "normal closure", CancellationToken.None);
            }
        }

        public HashSet<Guid> GetCall(Guid userId)
        {
            var call = _calls.Values.FirstOrDefault(c => c.Contains(userId));
            
            return call;
        }

        public void StartCall(Guid callerId)
        {
            _calls[callerId] = new HashSet<Guid> { callerId };
        }

        public void AddCallFriend(Guid callerId, Guid friendId)
        {
            _calls[callerId].Add(friendId);
        }

        public void RemoveCall(HashSet<Guid> call)
        {
            var pair = _calls.FirstOrDefault(p => p.Value == call);

            _calls.Remove(pair.Key, out call);
        }
    }
}