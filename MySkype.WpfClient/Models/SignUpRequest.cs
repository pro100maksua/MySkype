namespace MySkype.WpfClient.Models
{
    public class SignUpRequest
    {
        public string UserName { get; set; } = "Plotva";
        public string LastName { get; set; } 
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } = "Plotva";
    }
}