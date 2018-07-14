using Vanguard.ServerManager.Node.Abstractions;

namespace Vanguard.ServerManager.Node.Providers
{
    public class SteamInstanceOptions : ServerInstanceDefinition
    {
        public int GameId { get; set; }
        public int[] Mods { get; set; }
        public string SteamCredentialId { get; set; }
    }
}