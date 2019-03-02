using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySkype.WpfClient.Models;
using RestEase;

namespace MySkype.WpfClient.ApiInterfaces
{
    public interface IUserFriendsApi
    {
        [Header("Authorization")]
        string Token { get; set; }

        [Get]
        Task<IEnumerable<User>> GetFriendsAsync();

        [Post("{id}")]
        Task<bool> SendFriendRequestAsync([Path] Guid id);

        [Put("{id}")]
        Task<bool> ConfirmFriendRequestAsync([Path] Guid id);
    }
}