// Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BancaUT.Models;

namespace BancaUT.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // ── Tablas ──────────────────────────────────────
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Movimiento> Movimientos { get; set; }
        public DbSet<Presupuesto> Presupuestos { get; set; }
        public DbSet<MetaAhorro> MetasAhorro { get; set; }
        public DbSet<Recordatorio> Recordatorios { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // ⚠️ Siempre primero

            // ── Usuario → Categorías ─────────────────────
            builder.Entity<Categoria>()
                .HasOne(c => c.Usuario)
                .WithMany(u => u.Categorias)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade); // Si se borra usuario → se borran sus categorías

            // ── Usuario → Movimientos ────────────────────
            builder.Entity<Movimiento>()
                .HasOne(m => m.Usuario)
                .WithMany(u => u.Movimientos)
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction); // Evita conflicto de cascadas múltiples

            // ── Categoría → Movimientos ──────────────────
            builder.Entity<Movimiento>()
                .HasOne(m => m.Categoria)
                .WithMany(c => c.Movimientos)
                .HasForeignKey(m => m.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict); // No borrar categoría si tiene movimientos

            // ── Usuario → Presupuestos ───────────────────
            builder.Entity<Presupuesto>()
                .HasOne(p => p.Usuario)
                .WithMany(u => u.Presupuestos)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── Categoría → Presupuestos ─────────────────
            builder.Entity<Presupuesto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Presupuestos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Usuario → Metas ──────────────────────────
            builder.Entity<MetaAhorro>()
                .HasOne(m => m.Usuario)
                .WithMany(u => u.MetasAhorro)
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Usuario → Recordatorios ──────────────────
            builder.Entity<Recordatorio>()
                .HasOne(r => r.Usuario)
                .WithMany(u => u.Recordatorios)
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}