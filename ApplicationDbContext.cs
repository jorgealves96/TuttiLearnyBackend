using LearningAppNetCoreApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningAppNetCoreApi
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // --- Core User and Template Tables ---
        public DbSet<User> Users { get; set; }
        public DbSet<PathTemplate> PathTemplates { get; set; }
        public DbSet<PathItemTemplate> PathItemTemplates { get; set; }
        public DbSet<ResourceTemplate> ResourceTemplates { get; set; }

        // --- User Progress Tracking Tables ---
        public DbSet<UserPath> UserPaths { get; set; }
        public DbSet<UserResourceProgress> UserResourceProgress { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
