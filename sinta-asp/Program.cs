using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using sinta_asp.Services;
using sinta_asp.Models;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// 1. REGISTER SERVICES
// ==========================================================

// MVC & Controllers
builder.Services.AddControllersWithViews();

// Database (SQL Server)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Authentication & Cookies (PENTING BUAT LOGIN)
// Diambil dari branch SEMUA agar fitur login jalan
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
    });

// Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

// HttpContext & Session Configuration
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ==========================================================
// 2. BUILD APLIKASI
// ==========================================================
var app = builder.Build();

// ==========================================================
// 3. MIDDLEWARE / HTTP PIPELINE
// ==========================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// URUTAN INI SANGAT PENTING: Session -> Auth -> Authz
app.UseSession();
app.UseAuthentication(); // Wajib ada agar User.Identity terbaca
app.UseAuthorization();

// ==========================================================
// 4. ROUTING / MAPPING
// ==========================================================

// 1. Routing Khusus (Areas/Admin) - Dipertahankan dari request branch vava
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Login}/{action=Index}/{id?}");

// 2. Routing Umum (Halaman Depan/Default)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();