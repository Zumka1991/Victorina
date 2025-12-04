using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Victorina.Infrastructure.Data;

namespace Victorina.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<VictorinaDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}
