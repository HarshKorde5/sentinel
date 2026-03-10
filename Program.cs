using Sentinel.Data;
using Sentinel.Extensions;
using Sentinel.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddSentinelServices(builder.Configuration);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseAuthorization();
app.UseMiddleware<ApiKeyMiddleware>();
app.MapControllers();
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    await DatabaseSeeder.SeedAsync(db, config);
}

app.Run();