using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Vanguard.ServerManager.Core.Services;

namespace Vanguard.ServerManager.Core.Hubs
{
    [Authorize(Roles = RoleConstants.NodeAgent)]
    public class NodeHub : Hub
    {
        private readonly ILogger _logger;
        private readonly ServerNodeStatusService _statusService;

        public NodeHub(ILoggerFactory loggerFactory, ServerNodeStatusService statusService)
        {
            _logger = loggerFactory.CreateLogger<NodeHub>();
            _statusService = statusService;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("New connection from {0}", Context.UserIdentifier);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation("Lost connection from {0}", Context.UserIdentifier);
        }
    }
}