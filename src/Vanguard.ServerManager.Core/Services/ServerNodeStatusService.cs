using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Vanguard.ServerManager.Core.Api;
using Vanguard.ServerManager.Core.Extensions;

namespace Vanguard.ServerManager.Core.Services
{
    public class ServerNodeStatusService
    {
        private readonly IDistributedCache _cache;

        public ServerNodeStatusService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public Task UpdateStatusAsync(string nodeName, ServerNodeStatusViewModel status)
        {
            return _cache.SetObjectAsync(nodeName, status);
        }

        public async Task PatchStatusAsync(string nodeName, string propertyName, object value)
        {
            var fieldInfo = typeof(ServerNodeStatusViewModel).GetProperty(propertyName);
            var currentStatus = await _cache.GetObjectAsync<ServerNodeStatusViewModel>(nodeName);
            fieldInfo.SetValue(currentStatus, value);
            await UpdateStatusAsync(nodeName, currentStatus);
        }
    }
}