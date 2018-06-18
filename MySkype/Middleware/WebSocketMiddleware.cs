using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MySkype.Server.WebSocketManagers;

namespace MySkype.Server.Middleware
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebSocketManager _webSocketManager;
        private Guid _id;

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
                _id = new Guid(context.User.FindFirst("sid").Value);

                _webSocketManager.Add(_id, socket);

                await ReceiveAsync(socket);
            }
        }

        private async Task ReceiveAsync(WebSocket socket)
        {
            var buffer = new byte[4 * 1024];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                switch (result.MessageType)
                {
                    case WebSocketMessageType.Text:
                        await _webSocketManager.ReceiveTextAsync(result, buffer);
                        break;
                    case WebSocketMessageType.Binary:
                        await _webSocketManager.ReceiveBytesAsync(_id, result, buffer);
                        break;
                    case WebSocketMessageType.Close:
                        await _webSocketManager.RemoveAsync(_id);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
