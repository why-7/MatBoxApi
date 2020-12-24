using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Matbox.DAL.Models
{
    public sealed class MaterialsDbContext : DbContext
    {
        public DbSet<Material> Materials { get; set; }
        
        public MaterialsDbContext(DbContextOptions<MaterialsDbContext> options)
            : base(options)
        { 
            Database.EnsureCreated();
        }
    }
}