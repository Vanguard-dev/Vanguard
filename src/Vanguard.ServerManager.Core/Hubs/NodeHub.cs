using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Vanguard.ServerManager.Core.Services;

namespace Vanguard.ServerManager.Core.Hubs
{
    public class NodeHub : Hub
    {
        private readonly ServerNodeStatusService _statusService;

        public NodeHub(ServerNodeStatusService statusService)
        {
            _statusService = statusService;
        }

        public override async Task OnConnectedAsync()
        {
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
        }
    }
}