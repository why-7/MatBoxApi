using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Matbox.DAL.Models
{
    public sealed class AppDbContext : IdentityDbContext<User>
    {
        public DbSet<Material> Materials { get; set; }
        
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { 
            Database.EnsureCreated();
        }
    }
}