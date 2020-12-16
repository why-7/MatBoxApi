using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Matbox.Models
{
    public class UsersDbContext : IdentityDbContext<User>
    {
        public UsersDbContext(DbContextOptions<UsersDbContext> options)
            : base(options)
        { 
            Database.EnsureCreated();
        }
    }
}