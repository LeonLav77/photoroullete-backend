using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vjezba.Model;

namespace Vjezba.DAL
{
    public class ClientManagerDbContext : IdentityDbContext<AppUser>
    {
        protected ClientManagerDbContext() { }
        
        public ClientManagerDbContext(DbContextOptions<ClientManagerDbContext> options) : base(options)
        { }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}