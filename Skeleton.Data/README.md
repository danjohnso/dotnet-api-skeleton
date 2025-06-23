# Skeleton.Data
Currently configured for PostgreSQL

Data projects will need a dependency on the database provider project of choice

## Identity Integration
If you want to use with the Identity project and share the 'User' entity, here is a helpful pattern:
- Add a reference to the Identity project in your Data project.
- Add minimal mapping to 'User' in OnModelCreating in your DbContext:
```csharp
    base.OnModelCreating(modelBuilder);
    
    //ref to Identity project and ignore related entities
    modelBuilder.Entity<User>(entity =>
    {
        entity.ToTable(nameof(User));
        entity.HasKey(u => u.Id);
    });
    modelBuilder.Ignore<IdentityUserLogin<Guid>>();
    modelBuilder.Ignore<IdentityUserClaim<Guid>>();
    modelBuilder.Ignore<IdentityUserToken<Guid>>();
    modelBuilder.Ignore<AuditEvent>();
    modelBuilder.Ignore<PreviousPassword>();

    //add your mappings for the rest of your entities here
    ...
    //Map to UserProfile
    modelBuilder.Entity<UserProfile>().Map();
```
- Create navigation in your context to the UserProfile
```csharp
public DbSet<UserProfile> Users { get; set; }
```

- Delete the addition of the User table from your next migration in DataContext
 
**NOTE: Delete from the migration file but NOT from the context snapshot file.  You don't want to fight this on every migration**

You can still shoot yourself in the foot by accessing the User table with DataContext.Set<User>(), so don't be dumb or add these SaveChanges overrides to really lock it out:
```csharp
    public override int SaveChanges()
    {
        PreventUserModifications();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        PreventUserModifications();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void PreventUserModifications()
    {
        if (ChangeTracker.Entries<User>().Any(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
        {
            throw new InvalidOperationException("Modifications to User entities are not allowed in DataContext.");
        }
    }
```
