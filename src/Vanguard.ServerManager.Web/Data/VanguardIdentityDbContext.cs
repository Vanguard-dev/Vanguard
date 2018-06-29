using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vanguard.ServerManager.Web.Data.Entities;

namespace Vanguard.ServerManager.Web.Data
{
    public class VanguardIdentityDbContext : IdentityDbContext<VanguardUser, VanguardRole, string>
    {
        public VanguardIdentityDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}