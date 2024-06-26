using Bulky.Utility.IdentityUtils;
using Bulky.Utility.Payment;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Utility.Extensions
{
    public static class UtilityServices
    {
        public static IServiceCollection AddUtilityServices(this IServiceCollection services)
        {
            return services.AddScoped<IEmailSender, EmailSender>();
        }

        public static IServiceCollection ConfigurePaymentSettings(this IServiceCollection services, IConfiguration configuration)
            => services.Configure<StripeSettings>(configuration.GetSection("Stripe"));

        
    }
}
