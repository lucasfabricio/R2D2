using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using R2D2.Domain.Interfaces;
using R2D2.Application.Interfaces;
using R2D2.Infrastructure.Data;
using R2D2.Infrastructure.Repositories;
using R2D2.Infrastructure.Services;

namespace R2D2.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IPhysicalTableManager, PhysicalTableManager>();

        return services;
    }
}
