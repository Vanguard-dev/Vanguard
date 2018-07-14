using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;

namespace Vanguard.ServerManager.Core.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        private readonly OpenIddictApplicationManager<OpenIddictApplication> _applicationManager;

        public ApplicationController(OpenIddictApplicationManager<OpenIddictApplication> applicationManager)
        {
            _applicationManager = applicationManager;
        }

        [HttpGet("message")]
        [Authorize]
        public async Task<IActionResult> GetMessage()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return BadRequest();
            }

            return Content($"Great success!");
        }
    }
}