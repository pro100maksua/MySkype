﻿using System;
using System.Collections.Generic;

namespace MySkype.Server.Data.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public string UserName { get; set; }

        public string NormalizedUserName { get; set; }

        public string PasswordHash { get; set; }

        //public Guid AvatarId { get; set; }

        public Photo Avatar { get; set; }

        public IEnumerable<Guid> FriendIds { get; set; }

        public List<Guid> FriendRequests { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public User()
        {
            FriendIds = new List<Guid>();
            FriendRequests = new List<Guid>();
        }
    }
}