using System.Text;
using AWQP.Application.Interfaces;
using AWQP.Infrastructure.Persistence;
using AWQP.Infrastructure.Security;
using AWQP.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AWQP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddDbContext<ManufacturingDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("ManufacturingDb")));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IManufacturingService, ManufacturingService>();
        services.AddScoped<IQualityService, QualityService>();
        services.AddScoped<ICleanRoomService, CleanRoomService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IPpeService, PpeService>();
        services.AddScoped<IWorkPermitService, WorkPermitService>();
        services.AddScoped<ICheckSheetService, CheckSheetService>();
        services.AddSingleton<IBarcodeService, BarcodeService>();
        services.AddSingleton<IQrCodeService, QrCodeService>();

        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ClockSkew = TimeSpan.FromMinutes(2)
                };
            });
        return services;
    }
}
