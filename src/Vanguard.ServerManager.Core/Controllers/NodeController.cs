using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vanguard.ServerManager.Core.Api;
using Vanguard.ServerManager.Core.Entities;
using Vanguard.ServerManager.Core.Extensions;
using Vanguard.ServerManager.Core.Security;
using Vanguard.ServerManager.Core.Services;
using Vanguard.ServerManager.Node.Abstractions;

namespace Vanguard.ServerManager.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = RoleConstants.NodeAdmin)]
    public class NodeController : ControllerBase
    {
        private readonly ServerNodeService _service;
        private readonly UserManager<VanguardUser> _userManager;

        public NodeController(ServerNodeService service, UserManager<VanguardUser> userManager)
        {
            _service = service;
            _userManager = userManager;
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

            var username = Guid.NewGuid().ToString();
            var password = StringGenerator.GetRandomString(24);
            var user = new VanguardUser
            {
                UserName = username
            };
            var userResult = await _userManager.CreateAsync(user, password);
            if (!userResult.Succeeded)
            {
                ModelState.AddIdentityErrors(userResult.Errors);
                return BadRequest(ModelState);
            }
            await _userManager.AddToRoleAsync(user, RoleConstants.NodeAgent);

            model.UserId = user.Id;
            var createResult = await _service.CreateAsync(model);
            if (!createResult.Succeeded)
            {
                ModelState.AddEntityTransactionErrors(createResult.Errors);
                await _userManager.DeleteAsync(user);
                return BadRequest(ModelState);
            }

            return CreatedAtRoute("GetServerNode", new {id = createResult.Value.Id}, new UsernamePasswordCredentialsViewModel
            {
                Username = username,
                Password = password
            });
        }
    }
}