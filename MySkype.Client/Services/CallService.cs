using System;
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

        public async Task StartSendingBytesAsync(Guid targetId)
        {
            // NAudio ??
        }
    }
}
