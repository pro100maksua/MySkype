using System;

namespace MySkype.Client2.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public Photo Avatar { get; set; }

        public string Login { get; set; }

        public string Email { get; set; }
        
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName => FirstName + " " + LastName;
    }
}