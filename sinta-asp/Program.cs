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
        options.LoginPath = "/Admin/Login/Index"; // Disesuaikan dengan struktur Area Admin
        options.AccessDeniedPath = "/Home/AccessDenied";
    });

// Email Service
// Menggunakan AddScoped agar IEmailService merujuk ke EmailService
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
// 2. BUILD APLIKASI
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
app.UseStaticFiles();

app.UseRouting();

// Urutan Session, Auth, dan Authz ini SANGAT PENTING
app.UseSession(); 
app.UseAuthentication();
app.UseAuthorization();

// ==========================================================
// 4. ROUTING / MAPPING (URUTAN SANGAT PENTING)
// ==========================================================

// 1. Route Khusus Area Admin (Explicit)
app.MapAreaControllerRoute(
    name: "admin_area",
    areaName: "Admin",
    pattern: "Admin/{controller=Login}/{action=Index}/{id?}");

// 2. Route untuk Area secara umum
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// 3. Route Default (Luar Area / Peserta Magang)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();