using IptvConverter.Business.Services;
using IptvConverter.Business.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace IptvConverter.Business
{
    public static class Setup
    {
        public static IServiceCollection AddService(this IServiceCollection services)
        {
            services.AddTransient<IEpgService, EpgService>();
            services.AddTransient<IPlaylistService, PlaylistService>();

            return services;
        }

    }
}
