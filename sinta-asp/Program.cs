using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;
<<<<<<< HEAD
using sinta_asp.Models;

var builder = WebApplication.CreateBuilder(args);

// ===============================
// 1. REGISTER SERVICES
// ===============================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Menambahkan layanan Controller dengan Views
=======

var builder = WebApplication.CreateBuilder(args);

// --- SETTING KONEKSI MYSQL (LARAGON) ---
// Kita pakai yang ini karena sesuai dengan database kamu
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);
// ---------------------------------------

// Add services to the container.
>>>>>>> 659a81f9878d152c3c8220b7520b93e73f755cfb
builder.Services.AddControllersWithViews();

// Konfigurasi Session (Penting untuk Login)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
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

// Middleware Keamanan dan Session
app.UseSession();
app.UseAuthorization();

<<<<<<< HEAD
// ===============================
// 3. ROUTING (PENTING: JANGAN TERBALIK)
// ===============================

// 1. ROUTE UNTUK AREA (Admin)
// Rute ini akan menangani Controller di dalam folder Areas/Admin
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);

// 2. DEFAULT ROUTE (User / Halaman Depan)
// Rute ini menangani Controller utama di folder luar (HomeController)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);
=======
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
>>>>>>> 659a81f9878d152c3c8220b7520b93e73f755cfb

app.Run();