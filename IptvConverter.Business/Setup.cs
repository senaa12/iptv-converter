using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IptvConverter.Business.Services;
using IptvConverter.Business.Services.Interfaces;

namespace IptvConverter.Business
{
    public static class Setup
    {
        public static IServiceCollection AddService(this IServiceCollection services)
        {
            var serviceTypes = System.Reflection.Assembly.GetAssembly(typeof(IAppService)).GetTypes()
                    .Where(t => t.Namespace != null && t.Name.EndsWith("Service"));

            foreach (var intfc in serviceTypes.Where(t => t.IsInterface))
            {
                var impl = serviceTypes.FirstOrDefault(c => c.IsClass && intfc.Name.Substring(1) == c.Name);
                if (impl != null) services.AddScoped(intfc, impl);
            }

            return services;
        }

    }
}
