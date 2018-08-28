using System.Collections.Generic;
using System.Linq;

namespace Vanguard.Bot.Discord
{
    public class DiscordCommand
    {
        public string Name { get; private set; }
        public IEnumerable<string> Arguments { get; private set; }

        public DiscordCommand(string commandString)
        {
            var parts = commandString.Split(' ');
            Name = parts[0].Substring(1);
            Arguments = parts.Skip(1);
        }
    }
}