using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Skeleton.Identity.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace Skeleton.Identity
{
	public class AppIdentityContext : IdentityDbContext<User, Role, Guid>
	{
        public AppIdentityContext(DbContextOptions<AppIdentityContext> options)
            : base(options)
        {
        }

	    static AppIdentityContext()
	    {
		    AuditManager.DefaultConfiguration.AutoSavePreAction = (context, audit) =>
			    ((AppIdentityContext)context).AuditEntries.AddRange(audit.Entries.Where(x => x.AuditEntryID == 0));
	    }

        public DbSet<AuditEvent> AuditEvents { get; set; }
        public DbSet<PreviousPassword> PreviousPasswords { get; set; }
	    public DbSet<AuditEntry> AuditEntries { get; set; }
	    public DbSet<AuditEntryProperty> AuditEntryProperties { get; set; }
		
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //custom types
	        builder.Entity<User>().Map();
            builder.Entity<Role>().ToTable("Role"); //this is here for a stupid bug in 2.0 and AddEntityFrameworkStores()

            //default types
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserToken");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaim");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaim");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogin");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRole");

            //addons
	        builder.Entity<AuditEvent>().Map();
	        builder.Entity<PreviousPassword>().Map();
        }

	    public override int SaveChanges()
	    {
		    Audit audit = new Audit
		    {
			    //some things like audit events happen with unauthenticated users, so fallback to anonymous if we dont know
			    CreatedBy = this.GetService<IHttpContextAccessor>().HttpContext.User.FindFirst("sub")?.Value ?? "Anonymous"
		    };
			
		    audit.PreSaveChanges(this);
		    int rowsAffected = base.SaveChanges();
		    audit.PostSaveChanges();
			
		    if (audit.Configuration.AutoSavePreAction == null)
		    {
			    return rowsAffected;
		    }

		    audit.Configuration.AutoSavePreAction(this, audit);
		    base.SaveChanges();

		    return rowsAffected;
	    }

		public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	    {
			Audit audit = new Audit
			{
				//some things like audit events happen with unauthenticated users, so fallback to anonymous if we dont know
				CreatedBy = this.GetService<IHttpContextAccessor>().HttpContext.User.FindFirst("sub")?.Value ?? "Anonymous"
			};

			audit.PreSaveChanges(this);
		    int rowsAffected = await base.SaveChangesAsync(cancellationToken);
		    audit.PostSaveChanges();

		    if (audit.Configuration.AutoSavePreAction == null)
		    {
			    return rowsAffected;
		    }

		    audit.Configuration.AutoSavePreAction(this, audit);
		    await base.SaveChangesAsync(cancellationToken);

		    return rowsAffected;
	    }
    }
}
