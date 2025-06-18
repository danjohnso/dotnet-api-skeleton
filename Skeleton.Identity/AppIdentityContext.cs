using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Skeleton.Identity.Entities;

namespace Skeleton.Identity
{
	public class AppIdentityContext(DbContextOptions<AppIdentityContext> options) 
        : IdentityUserContext<User, Guid, IdentityUserClaim<Guid>, IdentityUserLogin<Guid>, IdentityUserToken<Guid>>(options)
	{
        public DbSet<AuditEvent> AuditEvents { get; set; }
        public DbSet<PreviousPassword> PreviousPasswords { get; set; }
		
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //custom types
	        builder.Entity<User>().Map();

            //this version does not use roles at all, they are generally better as claims
            //builder.Entity<IdentityRole<Guid>>().ToTable("Role");
            //builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRole");
            //builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaim");

            //default types
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserToken");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaim");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogin");

            //addons
	        builder.Entity<AuditEvent>().Map();
	        builder.Entity<PreviousPassword>().Map();
        }
    }

    /// <summary>
    /// UserContext without Roles
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TUserClaim"></typeparam>
    /// <typeparam name="TUserLogin"></typeparam>
    /// <typeparam name="TUserToken"></typeparam>
    public class IdentityUserContext<TUser, TKey, TUserClaim, TUserLogin, TUserToken> : DbContext
        where TUser : IdentityUser<TKey>
        where TKey : IEquatable<TKey>
        where TUserClaim : IdentityUserClaim<TKey>
        where TUserLogin : IdentityUserLogin<TKey>
        where TUserToken : IdentityUserToken<TKey>
    {
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        public IdentityUserContext(DbContextOptions options) : base(options) { }

        protected IdentityUserContext() { }
    }
}
