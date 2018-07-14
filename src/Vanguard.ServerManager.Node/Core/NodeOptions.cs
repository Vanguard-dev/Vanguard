namespace Vanguard.ServerManager.Node.Core
{
    public class NodeOptions
    {
        public bool IsInstalled { get; set; }
        public string CoreConnectionHostname { get; set; }
        public bool CoreConnectionNoSsl { get; set; }
        public bool CoreConnectionIgnoreSslWarnings { get; set; }

        public string ApiRoot => $"{(CoreConnectionNoSsl ? "http" : "https")}://{CoreConnectionHostname}";
    }
}