using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vanguard.ServerManager.Core.Api;
using Vanguard.ServerManager.Core.Extensions;
using Vanguard.ServerManager.Core.Services;

namespace Vanguard.ServerManager.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = RoleConstants.NodeAdmin)]
    public class NodeController : ControllerBase
    {
        private readonly ServerNodeService _service;

        public NodeController(ServerNodeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            return Ok(await _service.ToListAsync());
        }

        [HttpGet("{id}", Name = "GetServerNode")]
        public async Task<IActionResult> Get(string id)
        {
            return Ok(await _service.FindAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> RegisterNode([FromBody] ServerNodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _service.CreateAsync(model);
            if (!result.Succeeded)
            {
                ModelState.AddEntityTransactionErrors(result.Errors);
            }

            return CreatedAtRoute("GetServerNode", new {id = result.Value.Id}, result.Value);
        }
    }
}