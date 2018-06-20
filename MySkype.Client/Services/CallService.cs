using System;
using System.IO;
using System.Threading.Tasks;

namespace MySkype.Client.Services
{
    class CallService
    {
        private readonly WebSocketClient _webSocketClient;

        public CallService(WebSocketClient webSocketClient)
        {
            _webSocketClient = webSocketClient;
        }

        public async Task StartCallAsync(Guid targetId)
        {
            
        }
    }
}
