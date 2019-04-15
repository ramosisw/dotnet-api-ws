using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Common;

namespace Api.Controllers
{
    [Route("api/")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly WSMessageHandler _wsMessage;

        public HomeController(WSMessageHandler wsMessage)
        {
            _wsMessage = wsMessage;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return await Task.FromResult(new OkObjectResult(new
            {
                code = 200,
                message = "Message",
                clients = _wsMessage.Clients
            }));
        }

        [HttpGet]
        [Route("Invoke/{socketId}")]
        public async Task<IActionResult> Invoke(string socketId)
        {
            return Ok(await _wsMessage.Invoke(socketId, new Invocation { Id = Guid.NewGuid() }));
        }
    }
}