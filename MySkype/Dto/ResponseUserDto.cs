using System;
using System.Collections.Generic;
using MySkype.Server.Models;

namespace MySkype.Server.Dto
{
    public class ResponseUserDto
    {
        public Guid Id { get; set; }

        public Photo Avatar { get; set; }

        public List<Guid> FriendRequests { get; set; }

        public string Login { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public ResponseUserDto()
        {
            Avatar = new Photo();
            FriendRequests = new List<Guid>();
        }
    }
}
