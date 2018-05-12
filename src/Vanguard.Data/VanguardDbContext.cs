using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vanguard.Data.Entities;

namespace Vanguard.Data
{
    public class VanguardDbContext : IdentityDbContext<VanguardUser, VanguardRole, string>
    {
        public VanguardDbContext(DbContextOptions options) : base(options)
        {
        }

        protected VanguardDbContext()
        {
        }
    }
}