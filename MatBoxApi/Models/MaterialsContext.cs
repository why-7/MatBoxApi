using Microsoft.EntityFrameworkCore;

namespace MatBoxApi.Models
{
    public class MaterialsContext : DbContext
    {
        public DbSet<Material> Materials { get; set; }
        
        public MaterialsContext(DbContextOptions<MaterialsContext> options)
            : base(options)
        { 
            Database.EnsureCreated();
        }
    }
}