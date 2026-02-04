using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using sinta_asp.Services;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// 1. AREA REGISTRASI SERVICES (Sebelum builder.Build())
// ==========================================================

// MVC & Controllers
builder.Services.AddControllersWithViews();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Authentication & Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
    });

// Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

// HttpContext & Session
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ==========================================================
// 2. BUILD APLIKASI (Hanya boleh dipanggil SEKALI)
// ==========================================================
var app = builder.Build();

// ==========================================================
// 3. AREA MIDDLEWARE / HTTP PIPELINE (Setelah builder.Build())
// ==========================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Cukup panggil sekali saja

app.UseRouting();

// Urutan Session, Auth, dan Authz ini SANGAT PENTING
app.UseSession(); 
app.UseAuthentication();
app.UseAuthorization();

// ==========================================================
// 4. ROUTING / MAPPING
// ==========================================================

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Login}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();