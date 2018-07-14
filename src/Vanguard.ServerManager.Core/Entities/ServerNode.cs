namespace Vanguard.ServerManager.Core.Entities
{
    public class ServerNode
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string PublicKey { get; set; }
        public string UserId { get; set; }
        public virtual VanguardUser User { get; set; }
    }
}