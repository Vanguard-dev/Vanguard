using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vanguard.Web.Data.Entities;

namespace Vanguard.Web.Data
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