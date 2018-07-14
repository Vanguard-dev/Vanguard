using System.Net.Http;
using Vanguard.ServerManager.Node.Abstractions;

namespace Vanguard.ServerManager.Node.Core
{
    public class NodeHttpClient : HttpClient
    {
        private readonly Authenticator _authenticator;

        public bool IsReady => _authenticator.IsAuthenticated;

        public NodeHttpClient(Authenticator authenticator, NodeOptions nodeOptions) : base(new NodeHttpHandler(nodeOptions))
        {
            _authenticator = authenticator;
            DefaultRequestHeaders.Authorization = authenticator.AuthorizationHeader;
        }

        public class NodeHttpHandler : HttpClientHandler
        {
            public NodeHttpHandler(NodeOptions nodeOptions)
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                    nodeOptions.CoreConnectionIgnoreSslWarnings ||
                    new HttpClientHandler().ServerCertificateCustomValidationCallback(message, cert, chain, errors);
            }
        }
    }
}