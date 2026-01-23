using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;
using sinta_asp.Models;

var builder = WebApplication.CreateBuilder(args);

// ===============================
// 1. REGISTER SERVICES (DATABASE)
// ===============================

// KITA PAKAI SQL SERVER (PUNYA KAMU)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Service buat Controller & View
builder.Services.AddControllersWithViews();

// Konfigurasi Session (Penting buat Login)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ===============================
// 2. MIDDLEWARE
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

// Middleware Keamanan
app.UseSession();
app.UseAuthorization();

// ===============================
// 3. ROUTING KHUSUS & UMUM (JANGAN DIHAPUS)
// ===============================

// 🟢 1. INI ROUTING KHUSUS (ADMIN) YG KAMU MINTA PERTAHANKAN
// Tanpa ini, halaman Admin gak bakal bisa dibuka
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);

// 🔵 2. INI ROUTING UMUM (HALAMAN DEPAN)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();