using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MySkype.Server.Logic.Interfaces;

namespace MySkype.Server.Middleware
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebSocketManager _webSocketManager;

        public WebSocketMiddleware(RequestDelegate next, IWebSocketManager webSocketManager)
        {
            _next = next;
            _webSocketManager = webSocketManager;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next(context);
                return;
            }

            var socket = await context.WebSockets.AcceptWebSocketAsync();

            if (context.User.Identity.IsAuthenticated)
            {
                var userId = new Guid(context.User.Identity.Name);

                _webSocketManager.Add(userId, socket);

                await ReceiveAsync(userId, socket);
            }
        }

        private async Task ReceiveAsync(Guid userId, WebSocket socket)
        {
            var buffer = new byte[50 * 1024];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                switch (result.MessageType)
                {
                    case WebSocketMessageType.Text:
                        await _webSocketManager.ReceiveAsync(userId, result, buffer);
                        break;
                    case WebSocketMessageType.Close:
                        await _webSocketManager.RemoveAsync(userId);
                        break;
                }
            }
        }
    }
}