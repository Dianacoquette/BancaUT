// ============================================================
//  Data/SeedData.cs  —  BancaUT  |  Pre-llenado de BD
//  Cómo usar:
//    1. Coloca este archivo en la carpeta  Data/
//    2. En Program.cs agrega (justo antes de app.Run()):
//         await SeedData.InicializarAsync(app);
//    3. Ejecuta la aplicación en modo Development.
//    4. Listo — la BD se puebla automáticamente si está vacía.
// ============================================================

using BancaUT.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BancaUT.Data
{
    public static class SeedData
    {
        // ── Contraseña única para todos los usuarios demo ──────────────
        private const string PASSWORD_DEMO = "Demo@12345";

        public static async Task InicializarAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context      = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager  = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var logger       = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                // Aplicar migraciones pendientes automáticamente
                await context.Database.MigrateAsync();

                // Si ya existen usuarios, no volver a sembrar
                if (await context.Users.AnyAsync())
                {
                    logger.LogInformation("[Seed] Base de datos ya contiene datos. Se omite el seed.");
                    return;
                }

                logger.LogInformation("[Seed] Iniciando pre-llenado de base de datos...");

                // ────────────────────────────────────────────────────────
                //  1. USUARIOS DEMO
                // ────────────────────────────────────────────────────────
                var usuarios = new[]
                {
                    new ApplicationUser
                    {
                        UserName       = "ana.garcia@demo.com",
                        Email          = "ana.garcia@demo.com",
                        EmailConfirmed = true,
                        Nombre         = "Ana",
                        Apellido       = "García",
                        FechaRegistro  = new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                    },
                    new ApplicationUser
                    {
                        UserName       = "carlos.mendez@demo.com",
                        Email          = "carlos.mendez@demo.com",
                        EmailConfirmed = true,
                        Nombre         = "Carlos",
                        Apellido       = "Méndez",
                        FechaRegistro  = new DateTime(2025, 10, 15, 0, 0, 0, DateTimeKind.Utc),
                    },
                    new ApplicationUser
                    {
                        UserName       = "laura.torres@demo.com",
                        Email          = "laura.torres@demo.com",
                        EmailConfirmed = true,
                        Nombre         = "Laura",
                        Apellido       = "Torres",
                        FechaRegistro  = new DateTime(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc),
                    },
                };

                foreach (var u in usuarios)
                {
                    var result = await userManager.CreateAsync(u, PASSWORD_DEMO);
                    if (!result.Succeeded)
                        throw new Exception($"Error al crear usuario {u.Email}: " +
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                logger.LogInformation("[Seed] Usuarios creados: {Count}", usuarios.Length);

                // Recargar IDs asignados por Identity
                var ana    = await userManager.FindByEmailAsync("ana.garcia@demo.com")    ?? throw new Exception("Usuario Ana no encontrado");
                var carlos = await userManager.FindByEmailAsync("carlos.mendez@demo.com") ?? throw new Exception("Usuario Carlos no encontrado");
                var laura  = await userManager.FindByEmailAsync("laura.torres@demo.com")  ?? throw new Exception("Usuario Laura no encontrado");

                // ────────────────────────────────────────────────────────
                //  2. CATEGORÍAS
                // ────────────────────────────────────────────────────────
                var categorias = new List<Categoria>
                {
                    // ── Ana (usuario más completo) ─────────────────────
                    new() { UsuarioId = ana.Id, Nombre = "Sueldo",          Tipo = "Ingreso" },
                    new() { UsuarioId = ana.Id, Nombre = "Freelance",       Tipo = "Ingreso" },
                    new() { UsuarioId = ana.Id, Nombre = "Inversiones",     Tipo = "Ingreso" },
                    new() { UsuarioId = ana.Id, Nombre = "Renta / Vivienda",Tipo = "Egreso"  },
                    new() { UsuarioId = ana.Id, Nombre = "Alimentación",    Tipo = "Egreso"  },
                    new() { UsuarioId = ana.Id, Nombre = "Transporte",      Tipo = "Egreso"  },
                    new() { UsuarioId = ana.Id, Nombre = "Salud",           Tipo = "Egreso"  },
                    new() { UsuarioId = ana.Id, Nombre = "Entretenimiento", Tipo = "Egreso"  },
                    new() { UsuarioId = ana.Id, Nombre = "Ropa y Calzado",  Tipo = "Egreso"  },
                    new() { UsuarioId = ana.Id, Nombre = "Educación",       Tipo = "Egreso"  },
                    new() { UsuarioId = ana.Id, Nombre = "Servicios",       Tipo = "Egreso"  },

                    // ── Carlos ────────────────────────────────────────
                    new() { UsuarioId = carlos.Id, Nombre = "Sueldo",          Tipo = "Ingreso" },
                    new() { UsuarioId = carlos.Id, Nombre = "Negocio propio",  Tipo = "Ingreso" },
                    new() { UsuarioId = carlos.Id, Nombre = "Alimentación",    Tipo = "Egreso"  },
                    new() { UsuarioId = carlos.Id, Nombre = "Transporte",      Tipo = "Egreso"  },
                    new() { UsuarioId = carlos.Id, Nombre = "Entretenimiento", Tipo = "Egreso"  },
                    new() { UsuarioId = carlos.Id, Nombre = "Servicios",       Tipo = "Egreso"  },

                    // ── Laura ─────────────────────────────────────────
                    new() { UsuarioId = laura.Id, Nombre = "Sueldo",          Tipo = "Ingreso" },
                    new() { UsuarioId = laura.Id, Nombre = "Alimentación",    Tipo = "Egreso"  },
                    new() { UsuarioId = laura.Id, Nombre = "Transporte",      Tipo = "Egreso"  },
                    new() { UsuarioId = laura.Id, Nombre = "Salud",           Tipo = "Egreso"  },
                    new() { UsuarioId = laura.Id, Nombre = "Servicios",       Tipo = "Egreso"  },
                };

                context.Categorias.AddRange(categorias);
                await context.SaveChangesAsync();

                logger.LogInformation("[Seed] Categorías creadas: {Count}", categorias.Count);

                // Helper: buscar categoría por usuario y nombre
                Categoria Cat(string uid, string nombre) =>
                    categorias.First(c => c.UsuarioId == uid && c.Nombre == nombre);

                // ────────────────────────────────────────────────────────
                //  3. MOVIMIENTOS — 6 meses de historial para Ana
                //                   3 meses para Carlos y Laura
                // ────────────────────────────────────────────────────────
                var movimientos = new List<Movimiento>();
                var hoy = DateTime.Today;

                // ── Ana: octubre 2025 → marzo 2026 ────────────────────
                for (int mesOffset = 5; mesOffset >= 0; mesOffset--)
                {
                    var fecha = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-mesOffset);

                    // Ingresos
                    movimientos.Add(new() { UsuarioId = ana.Id, CategoriaId = Cat(ana.Id, "Sueldo").Id,     Tipo = "Ingreso", Monto = 18_000m, Fecha = fecha.AddDays(0),  Descripcion = "Pago quincena 1 + 2" });
                    movimientos.Add(new() { UsuarioId = ana.Id, CategoriaId = Cat(ana.Id, "Freelance").Id,  Tipo = "Ingreso", Monto = 3_500m,  Fecha = fecha.AddDays(14), Descripcion = "Proyecto diseño web" });
                    if (mesOffset % 3 == 0)
                        movimientos.Add(new() { UsuarioId = ana.Id, CategoriaId = Cat(ana.Id, "Inversiones").Id, Tipo = "Ingreso", Monto = 1_200m, Fecha = fecha.AddDays(20), Descripcion = "Rendimiento CETES" });

                    // Egresos fijos
                    movimientos.Add(new() { UsuarioId = ana.Id, CategoriaId = Cat(ana.Id, "Renta / Vivienda").Id, Tipo = "Egreso", Monto = 6_500m, Fecha = fecha.AddDays(1),  Descripcion = "Renta mensual" });
                    movimientos.Add(new() { UsuarioId = ana.Id, CategoriaId = Cat(ana.Id, "Servicios").Id,         Tipo = "Egreso", Monto = 850m,   Fecha = fecha.AddDays(5),  Descripcion = "Luz, agua, internet" });
                    movimientos.Add(new() { UsuarioId = ana.Id, CategoriaId = Cat(ana.Id, "Transporte").Id,        Tipo = "Egreso", Monto = 900m,   Fecha = fecha.AddDays(3),  Descripcion = "Gasolina y Uber" });

                    // Egresos variables
                    movimientos.Add(new() { UsuarioId = ana.Id, CategoriaId = Cat(ana.Id, "Alimentación").Id,    Tipo = "Egreso", Monto = 2_200m + (mesOffset * 100m), Fecha = fecha.AddDays(7),  Descripcion = "Súper del mes" });
                    movimientos.Add(new() { UsuarioId = ana.Id, CategoriaId = Cat(ana.Id, "Entretenimiento").Id, Tipo = "Egreso", Monto = 600m + (mesOffset * 50m),    Fecha = fecha.AddDays(15), Descripcion = "Streaming, salidas" });
                    movimientos.Add(new() { UsuarioId = ana.Id, CategoriaId = Cat(ana.Id, "Salud").Id,           Tipo = "Egreso", Monto = mesOffset % 2 == 0 ? 500m : 0m, Fecha = fecha.AddDays(10), Descripcion = "Farmacia y consulta" });
                    movimientos.Add(new() { UsuarioId = ana.Id, CategoriaId = Cat(ana.Id, "Educación").Id,       Tipo = "Egreso", Monto = 800m, Fecha = fecha.AddDays(2), Descripcion = "Curso online / libros" });

                    if (mesOffset % 2 == 1)
                        movimientos.Add(new() { UsuarioId = ana.Id, CategoriaId = Cat(ana.Id, "Ropa y Calzado").Id, Tipo = "Egreso", Monto = 1_100m, Fecha = fecha.AddDays(22), Descripcion = "Ropa de temporada" });
                }

                // ── Carlos: enero → marzo 2026 ────────────────────────
                for (int mesOffset = 2; mesOffset >= 0; mesOffset--)
                {
                    var fecha = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-mesOffset);

                    movimientos.Add(new() { UsuarioId = carlos.Id, CategoriaId = Cat(carlos.Id, "Sueldo").Id,         Tipo = "Ingreso", Monto = 22_000m, Fecha = fecha.AddDays(1),  Descripcion = "Nómina mensual" });
                    movimientos.Add(new() { UsuarioId = carlos.Id, CategoriaId = Cat(carlos.Id, "Negocio propio").Id, Tipo = "Ingreso", Monto = 5_000m,  Fecha = fecha.AddDays(20), Descripcion = "Ventas tienda en línea" });

                    movimientos.Add(new() { UsuarioId = carlos.Id, CategoriaId = Cat(carlos.Id, "Alimentación").Id,    Tipo = "Egreso", Monto = 3_000m, Fecha = fecha.AddDays(5),  Descripcion = "Despensa familiar" });
                    movimientos.Add(new() { UsuarioId = carlos.Id, CategoriaId = Cat(carlos.Id, "Transporte").Id,      Tipo = "Egreso", Monto = 1_200m, Fecha = fecha.AddDays(3),  Descripcion = "Gasolina" });
                    movimientos.Add(new() { UsuarioId = carlos.Id, CategoriaId = Cat(carlos.Id, "Servicios").Id,       Tipo = "Egreso", Monto = 1_100m, Fecha = fecha.AddDays(8),  Descripcion = "Servicios básicos" });
                    movimientos.Add(new() { UsuarioId = carlos.Id, CategoriaId = Cat(carlos.Id, "Entretenimiento").Id, Tipo = "Egreso", Monto = 800m,   Fecha = fecha.AddDays(16), Descripcion = "Restaurantes, cine" });
                }

                // ── Laura: febrero → marzo 2026 ───────────────────────
                for (int mesOffset = 1; mesOffset >= 0; mesOffset--)
                {
                    var fecha = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-mesOffset);

                    movimientos.Add(new() { UsuarioId = laura.Id, CategoriaId = Cat(laura.Id, "Sueldo").Id,        Tipo = "Ingreso", Monto = 15_000m, Fecha = fecha.AddDays(1),  Descripcion = "Quincenas" });
                    movimientos.Add(new() { UsuarioId = laura.Id, CategoriaId = Cat(laura.Id, "Alimentación").Id, Tipo = "Egreso",  Monto = 2_500m,  Fecha = fecha.AddDays(4),  Descripcion = "Súper y mercado" });
                    movimientos.Add(new() { UsuarioId = laura.Id, CategoriaId = Cat(laura.Id, "Transporte").Id,   Tipo = "Egreso",  Monto = 700m,    Fecha = fecha.AddDays(6),  Descripcion = "Transporte público y Uber" });
                    movimientos.Add(new() { UsuarioId = laura.Id, CategoriaId = Cat(laura.Id, "Salud").Id,        Tipo = "Egreso",  Monto = 400m,    Fecha = fecha.AddDays(10), Descripcion = "Vitaminas y chequeo" });
                    movimientos.Add(new() { UsuarioId = laura.Id, CategoriaId = Cat(laura.Id, "Servicios").Id,    Tipo = "Egreso",  Monto = 950m,    Fecha = fecha.AddDays(5),  Descripcion = "Luz, internet" });
                }

                // Filtrar movimientos con monto 0
                movimientos = movimientos.Where(m => m.Monto > 0).ToList();

                context.Movimientos.AddRange(movimientos);
                await context.SaveChangesAsync();

                logger.LogInformation("[Seed] Movimientos creados: {Count}", movimientos.Count);

                // ────────────────────────────────────────────────────────
                //  4. PRESUPUESTOS — mes actual para Ana y Carlos
                // ────────────────────────────────────────────────────────
                var mesActual = hoy.Month;
                var anioActual = hoy.Year;

                var presupuestos = new List<Presupuesto>
                {
                    // Ana
                    new() { UsuarioId = ana.Id, CategoriaId = Cat(ana.Id, "Alimentación").Id,    MontoLimite = 2_500m, Mes = mesActual, Anio = anioActual },
                    new() { UsuarioId = ana.Id, CategoriaId = Cat(ana.Id, "Entretenimiento").Id, MontoLimite = 800m,   Mes = mesActual, Anio = anioActual },
                    new() { UsuarioId = ana.Id, CategoriaId = Cat(ana.Id, "Transporte").Id,      MontoLimite = 1_000m, Mes = mesActual, Anio = anioActual },
                    new() { UsuarioId = ana.Id, CategoriaId = Cat(ana.Id, "Ropa y Calzado").Id,  MontoLimite = 600m,   Mes = mesActual, Anio = anioActual },
                    new() { UsuarioId = ana.Id, CategoriaId = Cat(ana.Id, "Salud").Id,           MontoLimite = 700m,   Mes = mesActual, Anio = anioActual },
                    // Carlos
                    new() { UsuarioId = carlos.Id, CategoriaId = Cat(carlos.Id, "Alimentación").Id,    MontoLimite = 3_200m, Mes = mesActual, Anio = anioActual },
                    new() { UsuarioId = carlos.Id, CategoriaId = Cat(carlos.Id, "Entretenimiento").Id, MontoLimite = 700m,   Mes = mesActual, Anio = anioActual },
                    new() { UsuarioId = carlos.Id, CategoriaId = Cat(carlos.Id, "Transporte").Id,      MontoLimite = 1_300m, Mes = mesActual, Anio = anioActual },
                };

                context.Presupuestos.AddRange(presupuestos);
                await context.SaveChangesAsync();

                logger.LogInformation("[Seed] Presupuestos creados: {Count}", presupuestos.Count);

                // ────────────────────────────────────────────────────────
                //  5. METAS DE AHORRO
                // ────────────────────────────────────────────────────────
                var metas = new List<MetaAhorro>
                {
                    // Ana
                    new() { UsuarioId = ana.Id,    Nombre = "Fondo de Emergencia",    MontoObjetivo = 50_000m, MontoActual = 18_500m, FechaInicio = new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc),  FechaObjetivo = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc)  },
                    new() { UsuarioId = ana.Id,    Nombre = "Vacaciones Europa 2026",  MontoObjetivo = 35_000m, MontoActual = 12_000m, FechaInicio = new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc), FechaObjetivo = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc)  },
                    new() { UsuarioId = ana.Id,    Nombre = "Laptop nueva",            MontoObjetivo = 22_000m, MontoActual = 22_000m, FechaInicio = new DateTime(2025, 8, 1, 0, 0, 0, DateTimeKind.Utc),  FechaObjetivo = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)  },
                    // Carlos
                    new() { UsuarioId = carlos.Id, Nombre = "Enganche depto",         MontoObjetivo = 120_000m, MontoActual = 45_000m, FechaInicio = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), FechaObjetivo = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    new() { UsuarioId = carlos.Id, Nombre = "Auto nuevo",             MontoObjetivo = 80_000m,  MontoActual = 15_000m, FechaInicio = new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc), FechaObjetivo = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc) },
                    // Laura
                    new() { UsuarioId = laura.Id,  Nombre = "Fondo de Emergencia",    MontoObjetivo = 20_000m, MontoActual = 3_000m,  FechaInicio = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),  FechaObjetivo = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc) },
                };

                context.MetasAhorro.AddRange(metas);
                await context.SaveChangesAsync();

                logger.LogInformation("[Seed] Metas de ahorro creadas: {Count}", metas.Count);

                // ────────────────────────────────────────────────────────
                //  6. RECORDATORIOS
                // ────────────────────────────────────────────────────────
                var recordatorios = new List<Recordatorio>
                {
                    // Ana — pendientes
                    new() { UsuarioId = ana.Id, Titulo = "Pagar tarjeta de crédito",     FechaRecordatorio = hoy.AddDays(3),  Completado = false, Descripcion = "Pago mínimo + extra para no generar intereses" },
                    new() { UsuarioId = ana.Id, Titulo = "Renovar seguro de auto",       FechaRecordatorio = hoy.AddDays(10), Completado = false, Descripcion = "Vence el 03/04/2026 — cotizar antes" },
                    new() { UsuarioId = ana.Id, Titulo = "Abonar meta Vacaciones",       FechaRecordatorio = hoy.AddDays(1),  Completado = false, Descripcion = "Transferir $2,000 al fondo de vacaciones" },
                    new() { UsuarioId = ana.Id, Titulo = "Pago de impuestos SAT",        FechaRecordatorio = hoy.AddDays(15), Completado = false, Descripcion = "Declaración mensual personas físicas" },
                    // Ana — completados
                    new() { UsuarioId = ana.Id, Titulo = "Pagar renta febrero",          FechaRecordatorio = hoy.AddDays(-20), Completado = true,  Descripcion = "Transferencia al propietario" },
                    new() { UsuarioId = ana.Id, Titulo = "Comprar despensa",             FechaRecordatorio = hoy.AddDays(-5),  Completado = true,  Descripcion = "Súper de la semana" },
                    // Carlos
                    new() { UsuarioId = carlos.Id, Titulo = "Pago de hipoteca",          FechaRecordatorio = hoy.AddDays(5),  Completado = false, Descripcion = "BBVA — pago mensual" },
                    new() { UsuarioId = carlos.Id, Titulo = "Renovar dominio web tienda",FechaRecordatorio = hoy.AddDays(20), Completado = false, Descripcion = "GoDaddy — renovación anual $800" },
                    new() { UsuarioId = carlos.Id, Titulo = "Pago de luz",               FechaRecordatorio = hoy.AddDays(-3),  Completado = true,  Descripcion = "CFE bimestral" },
                    // Laura
                    new() { UsuarioId = laura.Id, Titulo = "Pagar servicio médico",      FechaRecordatorio = hoy.AddDays(7),  Completado = false, Descripcion = "Cita de seguimiento" },
                    new() { UsuarioId = laura.Id, Titulo = "Renovar suscripción gym",    FechaRecordatorio = hoy.AddDays(12), Completado = false, Descripcion = "Pago mensual gimnasio" },
                };

                context.Recordatorios.AddRange(recordatorios);
                await context.SaveChangesAsync();

                logger.LogInformation("[Seed] Recordatorios creados: {Count}", recordatorios.Count);

                logger.LogInformation("[Seed] ✅ Pre-llenado completado exitosamente.");
                logger.LogInformation("[Seed] Usuarios de prueba:");
                logger.LogInformation("[Seed]   ana.garcia@demo.com    / {pass}", PASSWORD_DEMO);
                logger.LogInformation("[Seed]   carlos.mendez@demo.com / {pass}", PASSWORD_DEMO);
                logger.LogInformation("[Seed]   laura.torres@demo.com  / {pass}", PASSWORD_DEMO);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[Seed] ❌ Error durante el pre-llenado de datos.");
                throw;
            }
        }
    }
}
