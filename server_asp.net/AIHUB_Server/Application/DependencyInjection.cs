using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AIHUB_Server.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly()); // TOOD: FluentValidation, not used by now.
            services.AddMediatR(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
