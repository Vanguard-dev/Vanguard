using System.Collections.Generic;

namespace Vanguard.Bot.Discord
{
    public class DiscordBotConfig
    {
        public string ApiToken { get; set; }
        public string SelfAssignChannel { get; set; }
        public IEnumerable<string> AllowedSelfAssignRoles { get; set; }
    }
}