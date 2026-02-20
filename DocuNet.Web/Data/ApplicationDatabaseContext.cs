using DocuNet.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DocuNet.Web.Data
{
    public class ApplicationDatabaseContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public ApplicationDatabaseContext(DbContextOptions<ApplicationDatabaseContext> options)
            : base(options)
        {
        }

        public DbSet<Organization> Organizations { get; set; } = null!;
        public DbSet<Device> Devices { get; set; } = null!;
    }
}
