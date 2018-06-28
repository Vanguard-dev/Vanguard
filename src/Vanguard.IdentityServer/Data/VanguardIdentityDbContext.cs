using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vanguard.IdentityServer.Data.Entities;

namespace Vanguard.IdentityServer.Data
{
    public class VanguardIdentityDbContext : IdentityDbContext<VanguardUser, VanguardRole, string>
    {
        public VanguardIdentityDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}