using K8Intel.Models;
using Microsoft.EntityFrameworkCore;

namespace K8Intel.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Cluster> Clusters { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<ClusterMetric> ClusterMetrics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Cluster>()
                .HasMany(c => c.Alerts)
                .WithOne(a => a.Cluster)
                .HasForeignKey(a => a.ClusterId);

            modelBuilder.Entity<Cluster>()
                .HasMany(c => c.Metrics)
                .WithOne(m => m.Cluster)
                .HasForeignKey(m => m.ClusterId);
        }
    }
}