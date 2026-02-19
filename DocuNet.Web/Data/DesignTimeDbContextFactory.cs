using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DocuNet.Web.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDatabaseContext>
    {
        public ApplicationDatabaseContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDatabaseContext>();
            optionsBuilder.UseSqlite("Data Source=database.db");

            return new ApplicationDatabaseContext(optionsBuilder.Options);
        }
    }
}
