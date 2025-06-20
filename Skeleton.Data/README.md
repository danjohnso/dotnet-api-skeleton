# Skeleton.Data
Currently configured for PostgreSQL

Data projects will need a dependency on the database provider project of choice

## Identity Integration
If you want to use with the Identity project and share the 'User' entity, below are some changes that you will need to make:
- Add a reference to the Identity project in your Data project.
- Add minimal mapping to 'User' in OnModelCreating in your DbContext:
```csharp
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
```
- Override SaveChanges to prevent modifications since Identity should be managing changes to 'User'
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
        var userEntries = ChangeTracker.Entries<User>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);

        if (userEntries.Any())
        {
            throw new InvalidOperationException("Modifications to User entities are not allowed in DataContext.");
        }
    }
```
- Queries directly against 'Users' should use 'AsNoTracking'
```csharp
    context.Users.AsNoTracking();
```
-Delete the addition of the User table from your first migration in DataContext


The above works when the tables are all in the same database, if you ened to split the databases this can still work.  
One option would be to map 'User' to a cross database view in the DataContext
Another would be to have a minimal duplicated 'User' table that you synchronize with the Identity database using a database job or some other tool

Tradeoffs with both, will depend on what you are trying to implement
