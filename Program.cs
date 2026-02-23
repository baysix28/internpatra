using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using sinta_asp.Services;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// 1. AREA REGISTRASI SERVICES
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
        // Gunakan path login yang lebih umum atau sesuaikan jika Admin & Peserta loginnya beda.
        // Kita pakai path milik Sophie untuk support Area Admin.
        options.LoginPath = "/Admin/Login/Index"; 
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
// 2. BUILD APLIKASI
// ==========================================================
var app = builder.Build();

// ==========================================================
// 3. AREA MIDDLEWARE / HTTP PIPELINE
// ==========================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Urutan SANGAT PENTING: Session -> Auth -> Authz
app.UseSession(); 
app.UseAuthentication();
app.UseAuthorization();

// ==========================================================
// 4. ROUTING / MAPPING (Gabungan & Urutan Prioritas)
// ==========================================================

// 1. Prioritas Utama: Route Khusus Area Admin (Punya Sophie)
app.MapAreaControllerRoute(
    name: "admin_area",
    areaName: "Admin",
    pattern: "Admin/{controller=Login}/{action=Index}/{id?}");

// 2. Route Umum untuk semua Area yang ada (Punya Kamu & Sophie)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// 3. Route Terakhir: Default (Luar Area / Halaman Utama Peserta)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();