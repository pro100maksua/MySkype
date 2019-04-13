using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MySkype.Server.Data.Models;
using MySkype.Server.Logic.Dto;
using MySkype.Server.Logic.Interfaces;

namespace MySkype.Server.Logic.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<User> _userManager;
        private readonly IPhotoService _photoService;

        public IdentityService(IConfiguration config, UserManager<User> userManager, IPhotoService photoService)
        {
            _config = config;
            _userManager = userManager;
            _photoService = photoService;
        }

        public async Task<string> LoginAsync(TokenRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user != null && await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return GetUserToken(user);
            }

            return string.Empty;
        }

        public async Task<string> RegisterAsync(RegisterRequest registerRequest)
        {
            var user = registerRequest.Adapt<RegisterRequest, User>();

            var fileName = await _photoService.CreateDefaultAvatarAsync(user.FirstName, user.LastName);

            user.Id = Guid.NewGuid();
            user.Avatar = new Photo{Id = Guid.NewGuid(), FileName = fileName};

            var registerResult = await _userManager.CreateAsync(user, registerRequest.Password);
            if (registerResult.Succeeded)
            {
                return GetUserToken(user);
            }

            return string.Empty;
        }

        private string GetUserToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_config["Identity:SecurityKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                }),
                Issuer = _config["Domain"],
                Audience = _config["Domain"],
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }
    }
}
