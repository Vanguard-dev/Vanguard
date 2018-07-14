using System.Collections.Generic;

namespace Vanguard.Bot.Discord
{
    public class DiscordBotConfig
    {
        public string ApiToken { get; set; }
        public string SelfAssignChannel { get; set; }
        public IEnumerable<string> AllowedSelfAssignRoles { get; set; }
        public IEnumerable<ServerInfo> ServerList { get; set; }
    }

    public class ServerInfo
    {
        public string[] Keywords { get; set; }
        public string Description { get; set; }
    }
}