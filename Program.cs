using Microsoft.EntityFrameworkCore;
using sinta_asp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using sinta_asp.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

<<<<<<< HEAD
//////////////////////////////
// MVC + DB
//////////////////////////////
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
    ));

//////////////////////////////
// AUTHENTICATION (MULTI-SCHEME)
//////////////////////////////
builder.Services.AddAuthentication(options =>
{
    // Default tetap ke AdminScheme agar dashboard admin aman
    options.DefaultScheme = "AdminScheme";
    options.DefaultChallengeScheme = "AdminScheme";
    options.DefaultAuthenticateScheme = "AdminScheme";
=======
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
>>>>>>> vava4
})
.AddCookie("PesertaScheme", options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.Cookie.Name = "SINTA_PESERTA_AUTH";
<<<<<<< HEAD
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
=======
>>>>>>> vava4
})
.AddCookie("AdminScheme", options =>
{
    options.LoginPath = "/Admin/Login/Index";
<<<<<<< HEAD
    options.AccessDeniedPath = "/Admin/Login/Index";
    options.LogoutPath = "/Admin/Login/Logout";

    options.Cookie.Name = "SINTA_ADMIN_AUTH";

    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    options.ExpireTimeSpan = TimeSpan.FromHours(12);
    options.SlidingExpiration = false;

    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/Admin") &&
            context.Response.StatusCode == 200)
        {
            if (context.Request.Path.Value?.Contains("/Admin/Login", StringComparison.OrdinalIgnoreCase) == true)
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            }
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
=======
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.Cookie.Name = "SINTA_ADMIN_AUTH";
>>>>>>> vava4
});

//////////////////////////////
// AUTHORIZATION POLICIES
//////////////////////////////
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.AddAuthenticationSchemes("AdminScheme");
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Admin", "SuperAdmin");
    });

<<<<<<< HEAD
    options.AddPolicy("PesertaPolicy", policy =>
    {
        policy.AddAuthenticationSchemes("PesertaScheme");
        policy.RequireAuthenticatedUser();
    });

    options.AddPolicy("PesertaOnly", policy =>
    {
        policy.AddAuthenticationSchemes("PesertaScheme");
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Peserta");
    });
});

//////////////////////////////
// SERVICES + SESSION + SECURITY
//////////////////////////////
// Membaca setting email dari appsettings.json
builder.Services.AddScoped<IEmailService, EmailService>();

=======
// EMAIL SERVICE
builder.Services.AddScoped<IEmailService, EmailService>();

// HTTP CONTEXT ACCESSOR (Solusi Error InvalidOperationException Kamu!)
>>>>>>> vava4
builder.Services.AddHttpContextAccessor();

// SESSION
builder.Services.AddDistributedMemoryCache();

// Keamanan tambahan untuk form (Anti-CSRF)
builder.Services.AddAntiforgery(options => {
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

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

<<<<<<< HEAD
//////////////////////////////
// MIDDLEWARE
//////////////////////////////
=======
// ==========================================
// 3. MIDDLEWARE PIPELINE (app.Use...)
// ==========================================

>>>>>>> vava4
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Penting jika deploy di infrastruktur server perusahaan (Proxy/Load Balancer)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

<<<<<<< HEAD
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";

    await next();
});

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();


//////////////////////////////
// ROUTING
//////////////////////////////
=======
// Urutan Middleware Ini WAJIB BERURUTAN, Jangan Tertukar!
app.UseSession();         // 1. Session dulu
app.UseAuthentication();  // 2. Mengenali siapa yang login
app.UseAuthorization();   // 3. Mengecek hak aksesnya

// ==========================================
// 4. ROUTING & MAPPING
// ==========================================

>>>>>>> vava4
app.MapAreaControllerRoute(
    name: "admin_area",
    areaName: "Admin",
    pattern: "Admin/{controller=Login}/{action=Index}/{id?}");

app.MapControllerRoute(
<<<<<<< HEAD
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/test", () => "TEST MASUK SINI");

=======
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Jalankan Aplikasi
>>>>>>> vava4
app.Run();