using System;
using System.Collections.Generic;

namespace MySkype.WpfClient.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public Photo Avatar { get; set; }

        public List<Guid> FriendRequests { get; set; }

        public string Login { get; set; }

        public string Email { get; set; }
        
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName => FirstName + " " + LastName;

        public User()
        {
            Avatar = new Photo();
        }
    }
}