using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, logger) => logger.ReadFrom.Configuration(context.Configuration).WriteTo.Console());
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHttpClient("api", client => client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7001"));

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Dashboard}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
