using Microsoft.EntityFrameworkCore; // <-- Pastikan ada di paling atas
using sinta_asp.Data; // <-- Ini bakal merah sebentar, abaikan dulu
using Microsoft.AspNetCore.Authentication.Cookies;
using sinta_asp.Services;

var builder = WebApplication.CreateBuilder(args);


// --- SETTING KONEKSI SQL SERVER (SINTA) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
// ---------------------------------------

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "PesertaScheme";
})
.AddCookie("PesertaScheme", options =>
{
    // Sesuaikan path ini dengan URL halaman login buatan temanmu
    options.LoginPath = "/Login"; 
    options.AccessDeniedPath = "/Home/AccessDenied";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// 1. Authentication (Cek identitas / KTP) WAJIB LEBIH DULU
app.UseAuthentication(); 

// 2. Authorization (Cek hak akses / Tiket)
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();


// MVC
builder.Services.AddControllersWithViews();

// =========================
// DATABASE (FIX POOL + RETRY)
// =========================
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

// =========================
// AUTH: PESERTA + ADMIN
// =========================
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

// =========================
// EMAIL SERVICE
// =========================
builder.Services.AddScoped<IEmailService, EmailService>();

// =========================
// SESSION
// =========================
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
options.IdleTimeout = TimeSpan.FromMinutes(60);
options.Cookie.HttpOnly = true;
options.Cookie.IsEssential = true;
});


// =========================
// MIDDLEWARE
// =========================
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

// =========================
// ROUTING
// =========================
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


