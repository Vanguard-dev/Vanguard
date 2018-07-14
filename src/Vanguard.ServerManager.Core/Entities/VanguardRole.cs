using Microsoft.AspNetCore.Identity;

namespace Vanguard.ServerManager.Core.Entities
{
    public class VanguardRole : IdentityRole
    {
        public VanguardRole()
        {
        }

        public VanguardRole(string roleName) : base(roleName)
        {
        }
    }
}