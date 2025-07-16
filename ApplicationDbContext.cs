using LearningAppNetCoreApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningAppNetCoreApi
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<LearningPath> LearningPaths { get; set; }
        public DbSet<PathItem> PathItems { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Resource> Resources { get; set; }
    }
}
