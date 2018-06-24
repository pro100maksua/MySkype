using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySkype.Server.Models;

namespace MySkype.Server.Interfaces
{
    public interface ICallsRepository
    {
        Task<IEnumerable<Call>> GetUserCallsAsync(Guid userId);

        Task AddCallAsync(Call call);
    }
}
