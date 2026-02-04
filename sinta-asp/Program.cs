using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;
using sinta_asp.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. KONEKSI DATABASE (SQL SERVER)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// 2. REGISTER EMAIL SERVICE (FIXED)
// Gunakan AddScoped dengan Interface agar sinkron dengan Controller kamu
builder.Services.AddScoped<IEmailService, EmailService>();

// 3. HTTP CONTEXT ACCESSOR (Untuk keperluan Session/User context)
builder.Services.AddHttpContextAccessor();

// 4. SESSION CONFIGURATION
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 5. MVC CONFIGURATION
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
    });

// 6. CONFIGURE HTTP PIPELINE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); 
app.UseAuthentication();
app.UseAuthorization();

// 4. ROUTE
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Login}/{action=Index}/{id?}");

// Route Default
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseStaticFiles();

// PROGRAM BERHENTI DI SINI
app.Run(); 