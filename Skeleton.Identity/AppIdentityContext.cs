using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Skeleton.Identity.Entities;

namespace Skeleton.Identity
{
	public class AppIdentityContext(DbContextOptions<AppIdentityContext> options) 
        : IdentityDbContext<User, Guid, IdentityUserClaim<Guid>, IdentityUserLogin<Guid>, IdentityUserToken<Guid>>(options)
	{
        public DbSet<AuditEvent> AuditEvents { get; set; }
        public DbSet<PreviousPassword> PreviousPasswords { get; set; }
		
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //custom types
	        builder.Entity<User>().Map();
            
            
            //builder.Entity<IdentityRole<Guid>>().ToTable("Role");

            //default types
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserToken");
            //builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaim");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaim");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogin");
            //builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRole");

            //addons
	        builder.Entity<AuditEvent>().Map();
	        builder.Entity<PreviousPassword>().Map();
        }
    }
}
