using System.Threading.Tasks;

namespace Vanguard.ServerManager.Node.Abstractions
{
    public interface ICredentialsProvider
    {
        Task<T> GetCredentialsAsync<T>(string credentialsId);
    }

    public class UsernamePasswordCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}