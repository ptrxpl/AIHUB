using AIHUB_Server.Infrastructure.Configuration;
using AIHUB_Server.Application.Interfaces;
using AIHUB_Server.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AIHUB_Server.Application.Common.Interfaces;

namespace AIHUB_Server.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<ServerSettings>(config.GetSection("ServerSettings"));
            services.Configure<AuthSettings>(config.GetSection("AuthSettings"));

            services.AddTransient<IProjectFolderService, ProjectFolderService>();
            services.AddScoped<IUserService, UserService>();
            services.AddSingleton<ISingletonSignalRService, SingletonSignalRService>(); // TODO: database instead of singleton

            return services;
        }
    }
}
