using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;
<<<<<<< HEAD
using sinta_asp.Models;

var builder = WebApplication.CreateBuilder(args);

// ===============================
// 1. REGISTER SERVICES (DATABASE & SESSION)
// ===============================

// --- KONEKSI DATABASE (SQL SERVER - PUNYA KAMU) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
=======
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 1. KONEKSI KE DATABASE (SQL Server)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

>>>>>>> FEBRI-FIXX

// --- KONFIGURASI SESSION (Penting buat Login) ---
builder.Services.AddDistributedMemoryCache(); // Tambahan biar session makin lancar
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Login bertahan 60 menit
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Service buat Controller & View
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Alamat jika user maksa masuk tapi belum login
        options.AccessDeniedPath = "/Home/AccessDenied";
    });

var app = builder.Build();

// ===============================
// 2. MIDDLEWARE PIPELINE
// ===============================

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Middleware Wajib (Urutan: Session -> Auth)
app.UseSession();
app.UseAuthentication(); // Saya tambahkan ini jaga-jaga kalau nanti butuh
app.UseAuthorization();

// ===============================
// 3. ROUTING (KHUSUS & UMUM)
// ===============================

// 🟢 1. ROUTE KHUSUS ADMIN (AREAS)
// Kalau masuk ke /Admin, dia bakal cari Controller 'Login' atau 'Dashboard'
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Login}/{action=Index}/{id?}"
);

// 🔵 2. ROUTE UMUM (HALAMAN DEPAN)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();