using Microsoft.EntityFrameworkCore;

namespace BulkyWebRazor_Temp.Data.Extensions
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			var dbConnectionString = configuration.GetConnectionString("DefaultConnection")
				?? throw new ArgumentNullException(nameof(configuration));

			services.AddDbContext<ApplicationDbContext>(options =>
			{
				options.UseSqlServer(dbConnectionString);
			});
			return services;
		}
	}
}
