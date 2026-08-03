using System.Reflection;
using AWQP.Api.Middleware;
using AWQP.Api.Services;
using AWQP.Application;
using AWQP.Application.Interfaces;
using AWQP.Infrastructure;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, logger) => logger
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/awqp-api-.log", rollingInterval: RollingInterval.Day));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers().AddFluentValidation();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Production", policy => policy.RequireRole("Admin", "ProductionManager", "ProductionEngineer"));
    options.AddPolicy("Quality", policy => policy.RequireRole("Admin", "QualityEngineer"));
    options.AddPolicy("CleanRoom", policy => policy.RequireRole("Admin", "CleanRoomOperator", "QualityEngineer"));
    options.AddPolicy("Warehouse", policy => policy.RequireRole("Admin", "WarehouseStaff"));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "AWQP Semiconductor Quartz ERP/MES API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = Array.Empty<string>()
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", [AllowAnonymous] () => Results.Ok(new { status = "Healthy", service = "AWQP.Api", utc = DateTime.UtcNow }));
app.Run();
