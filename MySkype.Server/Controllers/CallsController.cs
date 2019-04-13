using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySkype.Server.Data.Models;
using MySkype.Server.Logic.Interfaces;

namespace MySkype.Server.Controllers
{
    [Authorize]
    [Route("api/calls")]
    [ApiController]
    public class CallsController : ControllerBase
    {
        private readonly ICallsService _callsService;

        public CallsController(ICallsService callsService)
        {
            _callsService = callsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserCallsAsync()
        {
            var id = new Guid(User.Identity.Name);

            var userCalls = await _callsService.GetUserCallsAsync(id);

            return Ok(userCalls);
        }

        [HttpPost]
        public async Task<IActionResult> SaveCallInfoAsync([FromBody] Call call)
        {
            await _callsService.SaveCallInfoAsync(call);

            return Ok();
        }

        [HttpGet("{callId}/participants")]
        public IActionResult GetCallParticipants(Guid callId)
        {
            var ids = _callsService.GetCallParticipants(callId);

            return Ok(ids);
        }
    }
}