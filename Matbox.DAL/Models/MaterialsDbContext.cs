using Microsoft.EntityFrameworkCore;

namespace Matbox.DAL.Models
{
    public sealed class MaterialsDbContext : DbContext
    {
        public DbSet<Material> Materials { get; set; }
        public DbSet<MaterialVersion> MaterialVersions { get; set; }

        public MaterialsDbContext(DbContextOptions<MaterialsDbContext> options)
            : base(options)
        { 
            
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Material>()
                .HasMany(x => x.Versions)
                .WithOne(x => x.Material)
                .HasForeignKey(x => x.MaterialId);
        }
    }
}