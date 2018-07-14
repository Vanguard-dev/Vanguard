namespace Vanguard.ServerManager.Node.Abstractions
{
    public class NodeOptions
    {
        public bool IsInstalled { get; set; }
        public string CoreConnectionHostname { get; set; }
        public bool CoreConnectionNoSsl { get; set; }
        public bool CoreConnectionIgnoreSslWarnings { get; set; }
    }
}