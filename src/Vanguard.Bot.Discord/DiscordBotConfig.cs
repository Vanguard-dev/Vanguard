using System.Collections.Generic;

namespace Vanguard.Bot.Discord
{
    public class DiscordBotConfig
    {
        public string ApiToken { get; set; }
        public SelfAssignRole SelfAssignRole { get; set; }
        public List<InfoCommand> InfoCommands { get; set; }
        public RulesAgreement RulesAgreement { get; set; }
    }

    public class RulesAgreement
    {
        public IEnumerable<string> ListenChannels { get; set; }
        public string Trigger { get; set; }
        public string Role { get; set; }
        public string Message { get; set; }
    }

    public class SelfAssignRole
    {
        public IEnumerable<string> ListenChannels { get; set; }
        public IEnumerable<string> AllowedSelfAssignRoles { get; set; }
    }

    public class InfoCommand
    {
        public IEnumerable<string> ListenChannels { get; set; }
        public string Trigger { get; set; }
        public IEnumerable<InfoEntry> Infos { get; set; }
    }

    public class InfoEntry
    {
        public string[] Keywords { get; set; }
        public string Description { get; set; }
    }
}