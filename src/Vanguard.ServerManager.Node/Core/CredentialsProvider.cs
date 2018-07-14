using System.Threading.Tasks;

namespace Vanguard.ServerManager.Node.Abstractions
{
    public class CredentialsProvider : ICredentialsProvider
    {
        public Task<T> GetCredentialsAsync<T>(string credentialsId)
        {
            // TODO: Fetch by id from core API
            // TODO: Attempt to deserialize to target type

            return null;
        }
    }
}