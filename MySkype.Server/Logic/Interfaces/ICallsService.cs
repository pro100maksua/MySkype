using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySkype.Server.Data.Models;

namespace MySkype.Server.Logic.Interfaces
{
    public interface ICallsService
    {
        Task<IEnumerable<Call>> GetUserCallsAsync(Guid userId);
        Task SaveCallInfoAsync(Call call);
        IEnumerable<Guid> GetCallParticipants(Guid callId);
    }
}