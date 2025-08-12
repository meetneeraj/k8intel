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
        
        public DbSet<Node> Nodes { get; set; }
        public DbSet<Pod> Pods { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<Recommendation> Recommendations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();



            modelBuilder.Entity<Cluster>()
                .HasIndex(c => c.Name)
                .HasMethod("gist") // Still specify GIST method
                .HasOperators("gist_trgm_ops") // 
                .HasDatabaseName("IX_Clusters_Name_Trgm");


            modelBuilder.Entity<Alert>()
                .HasIndex(a => new { a.ClusterId, a.Timestamp, a.Severity, a.IsResolved })
                .HasDatabaseName("IX_Alerts_ClusterId_Timestamp_Filtered"); // Optional: Give it a clear name



            modelBuilder.Entity<ClusterMetric>()
                .HasIndex(m => new { m.ClusterId, m.MetricType, m.Timestamp })
                .HasDatabaseName("IX_ClusterMetrics_ClusterId_Type_Timestamp");


            modelBuilder.Entity<Cluster>()
                .HasMany(c => c.Alerts)
                .WithOne(a => a.Cluster)
                .HasForeignKey(a => a.ClusterId);

            modelBuilder.Entity<Cluster>()
                .HasMany(c => c.Metrics)
                .WithOne(m => m.Cluster)
                .HasForeignKey(m => m.ClusterId);

            modelBuilder.Entity<Node>()
            .HasIndex(n => new { n.ClusterId, n.Name })
            .IsUnique();

            modelBuilder.Entity<Pod>()
                .HasIndex(p => new { p.NodeId, p.Name, p.Namespace })
                .IsUnique();

            modelBuilder.Entity<Cluster>()
                .HasMany(c => c.Nodes)
                .WithOne(n => n.Cluster)
                .HasForeignKey(n => n.ClusterId);

            modelBuilder.Entity<Node>()
                .HasMany(n => n.Pods)
                .WithOne(p => p.Node)
                .HasForeignKey(p => p.NodeId);

            modelBuilder.Entity<Incident>()
            .HasIndex(i => new { i.ClusterId, i.Fingerprint, i.Status });
            
            modelBuilder.Entity<Recommendation>()
                .HasIndex(r => new { r.ClusterId, r.Type });
        }
    }
}