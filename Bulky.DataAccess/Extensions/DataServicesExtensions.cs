using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Bulky.DataAccess.Repositories.Abstractions;

namespace Bulky.DataAccess.Extensions
{
    public static class DataServicesExtensions
    {
        public static IServiceCollection AddDbInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var dbConnectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(dbConnectionString);
            });
            return services;
        }

        public static IServiceCollection AddDataServices(this IServiceCollection services)
        {
            return services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
