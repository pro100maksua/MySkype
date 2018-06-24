using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySkype.Server.Interfaces;
using MySkype.Server.Models;

namespace MySkype.Server.Services
{
    public class CallsService
    {
        private readonly ICallsRepository _callsRepository;

        public CallsService(ICallsRepository callsRepository)
        {
            _callsRepository = callsRepository;
        }

        public async Task<IEnumerable<Call>> GetUserCallsAsync(Guid userId)
        {
            return await _callsRepository.GetUserCallsAsync(userId);
        }

        public async Task SaveCallInfoAsync(Call call)
        {
            await _callsRepository.AddCallAsync(call);
        }
    }
}
