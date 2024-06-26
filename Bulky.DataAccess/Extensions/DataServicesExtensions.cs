using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Bulky.DataAccess.Repositories.Abstractions;
using Microsoft.AspNetCore.Builder;
using Bulky.DataAccess.Initializer;

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
            services.AddScoped<IDbInitializer, DbInitializer>();
            return services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        public static void InitializeDatabase(this IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()!.CreateScope();
            var intializer = serviceScope.ServiceProvider.GetRequiredService<IDbInitializer>();
            intializer.Initialize();
        }
    }
}
