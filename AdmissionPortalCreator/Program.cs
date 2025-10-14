using AdmissionPortalCreator.Data;
using AdmissionPortalCreator.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------
// Database Configuration
// --------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --------------------------------------
// Identity Configuration
// --------------------------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// --------------------------------------
// Cookie Configuration
// --------------------------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// --------------------------------------
// MVC and Session
// --------------------------------------
builder.Services.AddControllersWithViews();

// ✅ Add distributed memory cache + session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// --------------------------------------
// Build app
// --------------------------------------
var app = builder.Build();

// --------------------------------------
// Middleware Pipeline
// --------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ✅ Enable session middleware
app.UseSession();

// --------------------------------------
// Routing Configuration
// --------------------------------------

// ✅ Map attribute-routed controllers (important for [HttpGet("/apply/...")])
app.MapControllers();

// ✅ Default route for admin/employee area
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// --------------------------------------
app.Run();
