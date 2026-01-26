using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 1. KONEKSI KE DATABASE (SQL Server)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// 2. KONFIGURASI SESSION (Wajib untuk Login)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Waktu login (60 menit)
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Tambahkan layanan MVC (Controllers with Views)
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Alamat jika user maksa masuk tapi belum login
        options.AccessDeniedPath = "/Home/AccessDenied";
    });

var app = builder.Build();

// 3. MIDDLEWARE PIPELINE (Urutan sangat penting!)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Wajib diletakkan di bawah UseRouting agar Session terbaca di Controller
app.UseSession(); 

app.UseAuthentication();
app.UseAuthorization();

// 4. KONFIGURASI ROUTE (Penting untuk Area)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Login}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();