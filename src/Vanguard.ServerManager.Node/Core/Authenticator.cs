using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Vanguard.ServerManager.Node.Abstractions;

namespace Vanguard.ServerManager.Node.Core
{
    public class Authenticator
    {
        private readonly NodeOptions _options;
        private IdentityTokens _tokens;

        public bool IsAuthenticated => _tokens != null && _tokens.ExpirationDate > DateTime.UtcNow;
        public string AccessToken => _tokens?.AccessToken;
        public AuthenticationHeaderValue AuthorizationHeader
        {
            get
            {
                if (_tokens == null)
                {
                    throw new InvalidOperationException("You need to authenticate before getting the AuthenticationHeader");
                }

                if (_tokens.ExpirationDate < DateTime.UtcNow)
                {
                    throw new AuthenticationException("The session is expired. You need to refresh the session or re-authenticate");
                }

                return new AuthenticationHeaderValue("Bearer", _tokens.AccessToken);
            }
        }

        public Authenticator(NodeOptions options)
        {
            _options = options;
        }

        public async Task<long> AuthenticateAsync(string username, string password)
        {
            _tokens = await FetchTokensAsync(new Dictionary<string, string> {{"username", username}, {"password", password}}, "password");
            return _tokens.ExpiresIn;
        }

        public async Task<long> RefreshAsync()
        {
            if (_tokens == null)
            {
                throw new InvalidOperationException("You need to authenticate before attempting to refresh the authentication");
            }

            _tokens = await FetchTokensAsync(new Dictionary<string, string> {{"refresh_token", _tokens.RefreshToken}}, "refresh_token");
            return _tokens.ExpiresIn;
        }

        private async Task<IdentityTokens> FetchTokensAsync(Dictionary<string, string> requestData, string grantType)
        {
            var encodedRequestData = string.Join('&', requestData.Select(t => $"{WebUtility.UrlEncode(t.Key)}={WebUtility.UrlEncode(t.Value)}"));
            var encodedPayload = $"{encodedRequestData}&grant_type={grantType}&scope=openid+offline_access";
            using (var client = new HttpClient(new NodeHttpClient.NodeHttpHandler(_options)))
            {
                var authResponse = await client.PostAsync($"{_options.ApiRoot}/connect/token", new StringContent(encodedPayload, Encoding.Default, "application/x-www-form-urlencoded"));
                if (!authResponse.IsSuccessStatusCode)
                {
                    throw new AuthenticationException($"Failed to authenticate the session: [{authResponse.StatusCode}] {await authResponse.Content.ReadAsStringAsync()}");
                }

                var tokens = JsonConvert.DeserializeObject<IdentityTokens>(await authResponse.Content.ReadAsStringAsync());
                tokens.ExpirationDate = DateTime.UtcNow + TimeSpan.FromSeconds(tokens.ExpiresIn);

                return tokens;
            }
        }
    }

    public class AuthenticationException : Exception
    {
        public AuthenticationException()
        {
        }

        public AuthenticationException(string message) : base(message)
        {
        }

        public AuthenticationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class IdentityTokens
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("id_token")]
        public string IdToken { get; set; }

        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        public DateTime ExpirationDate { get; set; }
    }
}