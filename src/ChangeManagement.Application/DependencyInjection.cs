using System.Reflection;
using ChangeManagement.Application.Mappings;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }
}
