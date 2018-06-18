using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MySkype.Client2
{
    public class WebSocketClient
    {
        private readonly string _token;
        private readonly ClientWebSocket _client;

        public WebSocketClient(string token)
        {
            _token = token;
            _client = new ClientWebSocket();
        }

        public async Task StartAsync()
        {
            _client.Options.SetRequestHeader("Authorization",
                "Bearer " + _token);

            await _client.ConnectAsync(new Uri("ws://localhost:5000/"), CancellationToken.None);

            await ReceiveAsync();
        }

        public async Task ReceiveAsync()
        {
            var buffer = new byte[4 * 1024];

            while (_client.State == WebSocketState.Open)
            {
                var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    break;
                }

                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                try
                {
                    var template = new { SenderId = Guid.Empty };
                    var sender = JsonConvert.DeserializeAnonymousType(json, template);
                }
                catch
                {
                    Console.WriteLine(json);
                }
            }
        }
    }
}
