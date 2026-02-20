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
        public DbSet<Connection> Connections { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Connection>(entity =>
            {
                // Configura o dispositivo de origem
                entity.HasOne(c => c.SourceDevice)
                      .WithMany()
                      .HasForeignKey(c => c.SourceDeviceId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Configura o dispositivo de destino
                entity.HasOne(c => c.DestinationDevice)
                      .WithMany()
                      .HasForeignKey(c => c.DestinationDeviceId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Configura a organização
                entity.HasOne(c => c.Organization)
                      .WithMany()
                      .HasForeignKey(c => c.OrganizationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Outras configurações de modelos podem ser adicionadas aqui
        }
    }
}
