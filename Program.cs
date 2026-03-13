// Program.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BancaUT.Data;
using BancaUT.Models;

var builder = WebApplication.CreateBuilder(args);

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 1. BASE DE DATOS
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 2. ASP.NET IDENTITY
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // ── Reglas de contraseña ──────────────────
    options.Password.RequireDigit = true;             // Al menos 1 número
    options.Password.RequireLowercase = true;          // Al menos 1 minúscula
    options.Password.RequireUppercase = true;          // Al menos 1 mayúscula
    options.Password.RequireNonAlphanumeric = false;   // Símbolo especial no obligatorio
    options.Password.RequiredLength = 8;               // Mínimo 8 caracteres

    // ── Bloqueo tras intentos fallidos ────────
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;       // 5 intentos → bloqueo
    options.Lockout.AllowedForNewUsers = true;

    // ── Email único por usuario ───────────────
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()  // Guardar todo en SQL Server
.AddDefaultTokenProviders();                        // Necesario para reset de contraseña

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 3. COOKIE DE SESIÓN
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";            // Si no está logueado → aquí
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);  // Sesión dura 8 horas
    options.SlidingExpiration = true;                // Se renueva con cada request
    options.Cookie.HttpOnly = true;                  // No accesible desde JavaScript
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 4. MVC
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 5. PIPELINE DE MIDDLEWARES
//    ⚠️ El orden aquí importa mucho
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();   // ← Leer la cookie e identificar al usuario
app.UseAuthorization();    // ← Verificar si tiene permiso para la ruta
                           // ⚠️ Authentication SIEMPRE antes de Authorization

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();