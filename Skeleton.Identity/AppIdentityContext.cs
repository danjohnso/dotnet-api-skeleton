using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Skeleton.Identity.Entities;

namespace Skeleton.Identity
{
	public class AppIdentityContext(DbContextOptions<AppIdentityContext> options) 
        : IdentityUserContext<User, Guid, IdentityUserClaim<Guid>, IdentityUserLogin<Guid>, UserToken>(options)
	{
        public DbSet<AuditEvent> AuditEvents { get; set; }
        public DbSet<PreviousPassword> PreviousPasswords { get; set; }
		
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //custom types
	        builder.Entity<User>().Map();
            builder.Entity<UserToken>().Map();

            //default types
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaim");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogin");

            //addons
	        builder.Entity<AuditEvent>().Map();
	        builder.Entity<PreviousPassword>().Map();
        }
    }
}
