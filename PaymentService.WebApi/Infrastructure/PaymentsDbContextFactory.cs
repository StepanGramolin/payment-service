using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PaymentService.WebApi.Infrastructure;

public sealed class PaymentsDbContextFactory : IDesignTimeDbContextFactory<PaymentsDbContext>
{
    public PaymentsDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cs = configuration.GetConnectionString("PaymentsDb")
                 ?? throw new InvalidOperationException("ConnectionStrings:PaymentsDb is required");

        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseNpgsql(cs)
            .Options;

        return new PaymentsDbContext(options);
    }
}