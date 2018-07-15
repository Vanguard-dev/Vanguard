using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vanguard.ServerManager.Core.Api;
using Vanguard.ServerManager.Core.Services;

namespace Vanguard.ServerManager.Core.Hubs
{
    [Authorize(Roles = RoleConstants.NodeAgent)]
    public class NodeHub : Hub
    {
        private readonly ILogger _logger;
        private readonly VanguardDbContext _dbContext;
        private readonly ServerNodeStatusService _statusService;

        public NodeHub(ILoggerFactory loggerFactory, VanguardDbContext dbContext, ServerNodeStatusService statusService)
        {
            _logger = loggerFactory.CreateLogger<NodeHub>();
            _dbContext = dbContext;
            _statusService = statusService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirst(JwtRegisteredClaimNames.Sub).Value;
            var serverNode = await _dbContext.ServerNodes.FirstOrDefaultAsync(t => t.UserId == userId);
            if (serverNode != null)
            {
                _logger.LogInformation("New connection from server node {0}", serverNode.Name);
                var status = await _statusService.PatchStatusAsync(serverNode, "Connected", true);
                await Clients.AllExcept(Context.ConnectionId).SendAsync("StatusUpdated", status);
            }
            else
            {
                _logger.LogInformation("New connection from user client {0}", userId);
                var serverNodeStatusList = await _statusService.ToStatusListAsync();
                await Clients.Client(Context.ConnectionId).SendAsync("UpdateStatusList", serverNodeStatusList);
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User.FindFirst(JwtRegisteredClaimNames.Sub).Value;
            var serverNode = await _dbContext.ServerNodes.FirstOrDefaultAsync(t => t.UserId == userId);
            if (serverNode != null)
            {
                _logger.LogInformation("Lost connection from server node {0}", serverNode.Name);
                var status = await _statusService.PatchStatusAsync(serverNode, "Connected", false);
                await Clients.AllExcept(Context.ConnectionId).SendAsync("StatusUpdated", status);
            }
            else
            {
                _logger.LogInformation("Lost connection from user client {0}", userId);
            }
        }
    }
}