using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;
using sinta_asp.Models;

var builder = WebApplication.CreateBuilder(args);

// ===============================
// 1. REGISTER SERVICES
// ===============================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Menambahkan layanan Controller dengan Views
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

app.Run();