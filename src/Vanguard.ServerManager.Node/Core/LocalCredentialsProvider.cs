using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Newtonsoft.Json;
using Vanguard.ServerManager.Node.Abstractions.Windows;

namespace Vanguard.ServerManager.Node.Abstractions
{
    public class LocalCredentialsProvider : ICredentialsProvider
    {
        public Task<T> GetCredentialsAsync<T>(string credentialsId)
        {
            try
            {
                // TODO: Platform separation
                var value = RegistryHelper.GetVanguardKey().GetValue(credentialsId) as string;
                return Task.FromResult(RegistryHelper.DecodeObject<T>(value));
            }
            catch (Exception ex)
            {
                throw new CredentialProviderException("Failed to write the credentials to registry", ex);
            }
        }

        public Task SetCredentialsAsync<T>(string credentialsId, T credentials)
        {
            try
            {
                // TODO: Platform separation
                RegistryHelper.GetVanguardKey().SetValue(credentialsId, RegistryHelper.EncodeObject(credentials));
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new CredentialProviderException("Failed to write the credentials to registry", ex);
            }
        }
    }
}