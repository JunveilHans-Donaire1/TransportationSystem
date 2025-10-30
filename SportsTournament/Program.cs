using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SportsTournament.Models;

var builder = WebApplication.CreateBuilder(args);

// ✅ Get connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("sql");

// ✅ Register DbContext with MySQL
builder.Services.AddDbContext<ManagementSystemDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ✅ Add Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Index";        // Redirect to login page
        options.AccessDeniedPath = "/Home/AccessDenied"; // Redirect if user is unauthorized
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Optional: cookie expiration
        options.SlidingExpiration = true;             // Optional: refresh cookie if user is active
    });

// ✅ Add MVC controllers with views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ✅ Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // ✅ Must come before Authorization
app.UseAuthorization();

// ✅ Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
