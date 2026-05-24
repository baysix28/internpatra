using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using sinta_asp.Services;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. SERVICES CONFIGURATION (builder.Services)
// ==========================================

// MVC & Controllers
builder.Services.AddControllersWithViews();

// DATABASE (FIX POOL + RETRY)
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
        }
    )
);

// AUTHENTICATION: PESERTA + ADMIN
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "PesertaScheme";
    options.DefaultChallengeScheme = "PesertaScheme";
    options.DefaultSignInScheme = "PesertaScheme";
})
.AddCookie("PesertaScheme", options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.Cookie.Name = "SINTA_PESERTA_AUTH";
})
.AddCookie("AdminScheme", options =>
{
    options.LoginPath = "/Admin/Login/Index";
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.Cookie.Name = "SINTA_ADMIN_AUTH";
});

builder.Services.AddAuthorization();

// EMAIL SERVICE
builder.Services.AddScoped<IEmailService, EmailService>();

// HTTP CONTEXT ACCESSOR (Solusi Error InvalidOperationException Kamu!)
builder.Services.AddHttpContextAccessor();

// SESSION
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ==========================================
// 2. BUILD THE APPLICATION
// ==========================================
var app = builder.Build();

// ==========================================
// 3. MIDDLEWARE PIPELINE (app.Use...)
// ==========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Urutan Middleware Ini WAJIB BERURUTAN, Jangan Tertukar!
app.UseSession();         // 1. Session dulu
app.UseAuthentication();  // 2. Mengenali siapa yang login
app.UseAuthorization();   // 3. Mengecek hak aksesnya

// ==========================================
// 4. ROUTING & MAPPING
// ==========================================

app.MapAreaControllerRoute(
    name: "admin_area",
    areaName: "Admin",
    pattern: "Admin/{controller=Login}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Jalankan Aplikasi
app.Run();