using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Vanguard.ServerManager.Core.Api;
using Vanguard.ServerManager.Core.Entities;
using Vanguard.ServerManager.Core.Extensions;

namespace Vanguard.ServerManager.Core.Services
{
    public class ServerNodeStatusService
    {
        private readonly IDistributedCache _cache;
        private readonly VanguardDbContext _dbContext;

        public ServerNodeStatusService(IDistributedCache cache, VanguardDbContext dbContext)
        {
            _cache = cache;
            _dbContext = dbContext;
        }

        public Task<ServerNodeStatusViewModel> GetStatusAsync(string nodeId, CancellationToken cancellationToken = default)
        {
            return _cache.GetObjectAsync<ServerNodeStatusViewModel>(nodeId, cancellationToken);
        }

        public Task SetStatusAsync(ServerNode node, ServerNodeStatusViewModel status, CancellationToken cancellationToken = default)
        {
            return _cache.SetObjectAsync(node.Id, status, cancellationToken);
        }

        public async Task<ServerNodeStatusViewModel> PatchStatusAsync(ServerNode node, string propertyName, object value, CancellationToken cancellationToken = default)
        {
            var fieldInfo = typeof(ServerNodeStatusViewModel).GetProperty(propertyName);
            var currentStatus = await _cache.GetObjectAsync(node.Id, new ServerNodeStatusViewModel { Id = node.Id, Name = node.Name }, cancellationToken);
            fieldInfo.SetValue(currentStatus, value);
            await SetStatusAsync(node, currentStatus, cancellationToken);
            return currentStatus;
        }

        public async Task<IEnumerable<ServerNodeStatusViewModel>> ToStatusListAsync(CancellationToken cancellationToken = default)
        {
            var serverNodeIds = await _dbContext.ServerNodes.Select(t => t.Id).ToListAsync(cancellationToken);
            return (await Task.WhenAll(serverNodeIds.Select(t => GetStatusAsync(t, cancellationToken)))).Where(t => t != null);
        }
    }
}