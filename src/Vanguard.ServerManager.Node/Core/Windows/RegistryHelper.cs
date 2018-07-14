using System;
using System.Text;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace Vanguard.ServerManager.Node.Abstractions.Windows
{
    public static class RegistryHelper
    {
        public static string EncodeObject(object value)
        {
            var serializedValue = JsonConvert.SerializeObject(value);
            return Convert.ToBase64String(Encoding.Unicode.GetBytes(serializedValue));
        }

        public static T DecodeObject<T>(string value)
        {
            var serializedValue = Encoding.Unicode.GetString(Convert.FromBase64String(value));
            return JsonConvert.DeserializeObject<T>(serializedValue);
        }

        public static RegistryKey GetVanguardKey()
        {
            var userSoftwareRegistryKey = Registry.CurrentUser.OpenSubKey("Software", true);
            if (userSoftwareRegistryKey == null)
            {
                throw new CredentialProviderException("Unable to open HKLM_CURRENT_USER\\Software for credentials storage");
            }

            userSoftwareRegistryKey.CreateSubKey("Vanguard");
            var vanguardRegistryKey = userSoftwareRegistryKey.OpenSubKey("Vanguard", true);
            if (vanguardRegistryKey == null)
            {
                throw new CredentialProviderException("Unable to open HKLM_CURRENT_USER\\Software\\Vanguard for credentials storage");
            }

            return vanguardRegistryKey;
        }
    }
}