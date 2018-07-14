using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vanguard.ServerManager.Core.Entities;

namespace Vanguard.ServerManager.Core
{
    public class VanguardDbContext : IdentityDbContext<VanguardUser, VanguardRole, string>
    {
        public DbSet<ServerNode> ServerNodes { get; set; }

        public VanguardDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ServerNode>(entity =>
            {
                entity.HasIndex(t => t.Name)
                    .IsUnique();

                entity.HasOne(t => t.User)
                    .WithOne(t => t.ServerNode)
                    .HasForeignKey<ServerNode>(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(t => t.PublicKey)
                    .IsRequired();
            });

            builder.Entity<VanguardRole>().HasData(
                new VanguardRole(RoleConstants.NodeAdmin) { NormalizedName = RoleConstants.NodeAdmin.ToUpper() },
                new VanguardRole(RoleConstants.UserAdmin) { NormalizedName = RoleConstants.UserAdmin.ToUpper() },

                new VanguardRole(RoleConstants.NodeAgent) { NormalizedName = RoleConstants.NodeAgent.ToUpper() }
            );
        }
    }
}