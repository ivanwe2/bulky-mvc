using Bulky.Utility.IdentityUtils;
using Microsoft.AspNetCore.Identity.UI.Services;
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
    }
}
