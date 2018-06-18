using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySkype.Server.Services;

namespace MySkype.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly IdentityService _identityService;

        public IdentityController(IdentityService identityService)
        {
            _identityService = identityService;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> RequestToken([FromBody] TokenRequest request)
        {
            var token = await _identityService.RequestTokenAsync(request);

            if (token == null)
                return BadRequest("Invalid login or password.");

            return Ok(token);
        }
    }
}
