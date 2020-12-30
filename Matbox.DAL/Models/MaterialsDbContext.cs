using Microsoft.EntityFrameworkCore;

namespace Matbox.DAL.Models
{
    public sealed class MaterialsDbContext : DbContext
    {
        public DbSet<Material> Materials { get; set; }
        
        public MaterialsDbContext(DbContextOptions<MaterialsDbContext> options)
            : base(options)
        { 
            
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Material>();
        }
    }
}