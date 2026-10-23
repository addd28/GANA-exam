using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SmartShop.API.Data;

public class SmartShopDbContextFactory : IDesignTimeDbContextFactory<SmartShopDbContext>
{
    public SmartShopDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost;Database=SMARTSHOP;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

        var optionsBuilder = new DbContextOptionsBuilder<SmartShopDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new SmartShopDbContext(optionsBuilder.Options);
    }
}
