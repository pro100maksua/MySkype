using System;
using System.Collections.Generic;
using MySkype.Server.Data.Models;

namespace MySkype.Server.Logic.Dto
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }

        public Photo Avatar { get; set; }

        public List<Guid> FriendRequests { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public UserResponseDto()
        {
            Avatar = new Photo();
            FriendRequests = new List<Guid>();
        }
    }
}
