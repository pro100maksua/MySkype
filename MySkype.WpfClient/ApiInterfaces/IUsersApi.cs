using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySkype.WpfClient.Models;
using RestEase;

namespace MySkype.WpfClient.ApiInterfaces
{
    public interface IUsersApi
    {
        [Header("Authorization")]
        string Token { get; set; }

        [Get]
        Task<IEnumerable<User>> GetAllAsync([Query] string searchString);

        [Get("{id}")]
        Task<User> GetUserAsync([Path] Guid id);
        
        [Post]
        Task<Response<User>> RegisterAsync([Body] SignUpRequest registerForm);

        [Get("{id}/isOnline")]
        Task<bool> CheckIfUserOnlineAsync([Path] Guid id);
    }
}