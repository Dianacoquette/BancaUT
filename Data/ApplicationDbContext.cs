// Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BancaUT.Models;

namespace BancaUT.Data
{
    // IdentityDbContext<ApplicationUser> se encarga de crear
    // automáticamente TODAS las tablas que Identity necesita:
    //
    //   AspNetUsers        → tus usuarios
    //   AspNetRoles        → roles (Admin, Cliente, etc.)
    //   AspNetUserRoles    → qué rol tiene cada usuario
    //   AspNetUserClaims   → permisos adicionales
    //   AspNetUserTokens   → tokens de reset de contraseña
    //   AspNetUserLogins   → logins externos (Google, etc.)

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // ⚠️ OBLIGATORIO llamar a base — configura todas las tablas de Identity
            base.OnModelCreating(builder);

            // Aquí agregarás tus otras tablas en módulos futuros, ejemplo:
            // builder.Entity<Cuenta>().ToTable("Cuentas");
        }
    }
}