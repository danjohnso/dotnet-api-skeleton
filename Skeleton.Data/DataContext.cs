using Skeleton.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Skeleton.Data
{
    public partial class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppFile>().Map();
            modelBuilder.Entity<Thing>().Map();
            modelBuilder.Entity<User>().Map();

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<AppFile> AppFiles { get; set; }
        public DbSet<Thing> Things { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
