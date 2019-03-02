using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySkype.WpfClient.Models;
using RestEase;

namespace MySkype.WpfClient.ApiInterfaces
{
    public interface ICallsApi
    {
        [Header("Authorization")]
        string Token { get; set; }

        [Get]
        Task<IEnumerable<Call>> GetCallsAsync();
        
        [Post]
        Task SaveCallInfoAsync([Body] Call registerForm);

        [Get("{id}/participants")]
        Task<IEnumerable<Guid>> GetCallParticipantsAsync([Path] Guid id);
    }
}