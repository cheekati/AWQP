using Serilog;
using AWQP.Web.Components;
using AWQP.Web.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, logger) => logger.ReadFrom.Configuration(context.Configuration).WriteTo.Console());
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<ApiTokenProvider>();
builder.Services.AddScoped<CheckSheetApiClient>();
builder.Services.AddScoped<CheckSheetUploadService>();
builder.Services.AddHttpClient("api", client => client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7001"));
builder.Services.AddHttpClient();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Dashboard}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();
