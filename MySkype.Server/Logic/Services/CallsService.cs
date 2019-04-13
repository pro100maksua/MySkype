using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySkype.Server.Data.Interfaces;
using MySkype.Server.Data.Models;
using MySkype.Server.Logic.Interfaces;
using MySkype.Server.Logic.WebSocketManagers;

namespace MySkype.Server.Logic.Services
{
    public class CallsService : ICallsService
    {
        private readonly ICallsRepository _callsRepository;
        private readonly WebSocketManager _webSocketManager;

        public CallsService(ICallsRepository callsRepository, WebSocketManager webSocketManager)
        {
            _callsRepository = callsRepository;
            _webSocketManager = webSocketManager;
        }

        public async Task<IEnumerable<Call>> GetUserCallsAsync(Guid userId)
        {
            return await _callsRepository.GetUserCallsAsync(userId);
        }

        public async Task SaveCallInfoAsync(Call call)
        {
            await _callsRepository.AddCallAsync(call);
        }

        public IEnumerable<Guid> GetCallParticipants(Guid callId)
        {
            return _webSocketManager.GetCallParticipants(callId);
        }
    }
}
