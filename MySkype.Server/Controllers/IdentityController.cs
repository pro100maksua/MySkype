using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MySkype.Server.Data.Models;
using MySkype.Server.Logic.Dto;
using MySkype.Server.Logic.Interfaces;

namespace MySkype.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        public IdentityController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> LoginAsync([FromBody] TokenRequest request)
        {
            var token = await _identityService.LoginAsync(request);
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Invalid login or password.");
            }

            return token;
        }
        
        [HttpPost("register")]
        public async Task<ActionResult<string>> RegisterAsync([FromBody]RegisterRequest registerRequest)
        {
            var token = await _identityService.RegisterAsync(registerRequest);
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest();
            }
            
            return Ok(token);
        }
    }
}
