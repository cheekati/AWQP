using System.Reflection;
using AWQP.Application.Common;
using AWQP.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AWQP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<CustomerService>();
        services.AddScoped<ProductService>();
        services.AddScoped<WorkOrderService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ITraceabilityService, TraceabilityService>();
        return services;
    }
}
