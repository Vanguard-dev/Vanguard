using System;
using System.Threading.Tasks;

namespace Vanguard.ServerManager.Node.Abstractions
{
    public interface ICredentialsProvider
    {
        Task<T> GetCredentialsAsync<T>(string credentialsId);
        Task SetCredentialsAsync<T>(string credentialsId, T credentials);
    }

    public class CredentialProviderException : Exception
    {
        public CredentialProviderException()
        {
        }

        public CredentialProviderException(string message) : base(message)
        {
        }

        public CredentialProviderException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}