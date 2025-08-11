using K8Intel.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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

            // 1. Create a UNIQUE index on the Username column.
            // This ensures no two users can have the same username and makes login lookups extremely fast.
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();


            // === Cluster Table Indexes ===

            // 2. Add an index to the Name column to speed up searching.
            // Note: For advanced "CONTAINS" / "LIKE" searches in PostgreSQL, 
            // a trigram index would be even better, but that requires a DB extension.
            // This standard B-tree index will still significantly help with 'StartsWith' queries.
            modelBuilder.Entity<Cluster>()
                .HasIndex(c => c.Name)
                .HasMethod("gist") // Still specify GIST method
                .HasOperators("gist_trgm_ops") // 
                .HasDatabaseName("IX_Clusters_Name_Trgm");

            // === Alert Table Indexes ===

            // 3. Create a composite index for the most common query pattern on Alerts.
            // We query by ClusterId, then filter by Severity/Status, and order by Timestamp.
            // Placing ClusterId first is most important as nearly every query is scoped to it.
            modelBuilder.Entity<Alert>()
                .HasIndex(a => new { a.ClusterId, a.Timestamp, a.Severity, a.IsResolved })
                .HasDatabaseName("IX_Alerts_ClusterId_Timestamp_Filtered"); // Optional: Give it a clear name

            // An alternative simple FK index if the composite one is too much. EF Core often creates this
            // implicitly, but being explicit is good practice.
            // modelBuilder.Entity<Alert>().HasIndex(a => a.ClusterId);


            // === ClusterMetric Table Indexes ===

            // 4. Create a composite index for the most common query pattern on Metrics.
            // Querying is always by ClusterId, then often by MetricType, and sorted/ranged by Timestamp.
            modelBuilder.Entity<ClusterMetric>()
                .HasIndex(m => new { m.ClusterId, m.MetricType, m.Timestamp })
                .HasDatabaseName("IX_ClusterMetrics_ClusterId_Type_Timestamp");

            // modelBuilder.Entity<ClusterMetric>().HasIndex(m => m.ClusterId);


            // === Existing Relationships (Keep these) ===
            
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