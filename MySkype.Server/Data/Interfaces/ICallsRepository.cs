using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySkype.Server.Data.Models;

namespace MySkype.Server.Data.Interfaces
{
    public interface ICallsRepository
    {
        Task<IEnumerable<Call>> GetUserCallsAsync(Guid userId);

        Task AddCallAsync(Call call);
    }
}
