using System;
using System.Threading.Tasks;

namespace MySkype.WpfClient.Services
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
