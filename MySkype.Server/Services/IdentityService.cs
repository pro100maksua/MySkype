using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MySkype.Server.Interfaces;

namespace MySkype.Server.Services
{
    public class IdentityService
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IConfiguration _config;

        public IdentityService(IUsersRepository usersRepository, IConfiguration config)
        {
            _usersRepository = usersRepository;
            _config = config;
        }

        public async Task<string> RequestTokenAsync(TokenRequest request)
        {
            var user = await _usersRepository.GetAsync(request.Login, request.Password);

            if (user == null) return null;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Identity:SecurityKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sid, user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                _config["Domain"],
                _config["Domain"], claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
