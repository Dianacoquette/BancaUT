using BancaUT.Data;
using BancaUT.Models;
using BancaUT.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BancaUT.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var movimientos = await _context.Movimientos
                .Where(x => x.UsuarioId == user.Id)
                .Include(x => x.Categoria)
                .OrderByDescending(x => x.Fecha)
                .ThenByDescending(x => x.Id)
                .ToListAsync();

            var ingresos = movimientos.Where(x => x.Tipo == "Ingreso").Sum(x => x.Monto);
            var gastos = movimientos.Where(x => x.Tipo == "Egreso").Sum(x => x.Monto);
            var saldo = ingresos - gastos;

            var hoy = DateTime.Today;
            var presupuestos = await _context.Presupuestos
                .Include(x => x.Categoria)
                .Where(x => x.UsuarioId == user.Id && x.Mes == hoy.Month && x.Anio == hoy.Year)
                .ToListAsync();

            var alertas = presupuestos
                .Select(p =>
                {
                    var gastado = movimientos
                        .Where(m => m.Tipo == "Egreso" && m.CategoriaId == p.CategoriaId && m.Fecha.Month == p.Mes && m.Fecha.Year == p.Anio)
                        .Sum(m => m.Monto);

                    return new PresupuestoAlertaItem
                    {
                        PresupuestoId = p.Id,
                        Categoria = p.Categoria?.Nombre ?? "Sin categoría",
                        Limite = p.MontoLimite,
                        Gastado = gastado,
                        PorcentajeUsado = p.MontoLimite > 0 ? Math.Round((gastado / p.MontoLimite) * 100, 1) : 0
                    };
                })
                .Where(x => x.PorcentajeUsado >= 80)
                .OrderByDescending(x => x.PorcentajeUsado)
                .ToList();

            var model = new DashboardViewModel
            {
                Ingresos = ingresos,
                Gastos = gastos,
                Saldo = saldo,
                AhorradoMetas = await _context.MetasAhorro.Where(x => x.UsuarioId == user.Id).SumAsync(x => x.MontoActual),
                GastosPorCategoria = movimientos
                    .Where(x => x.Tipo == "Egreso" && x.Categoria != null)
                    .GroupBy(x => x.Categoria!.Nombre)
                    .Select(g => new GastoCategoriaItem
                    {
                        Categoria = g.Key,
                        Total = g.Sum(x => x.Monto)
                    })
                    .OrderByDescending(x => x.Total)
                    .Take(6)
                    .ToList(),
                MovimientosRecientes = movimientos.Take(8).ToList(),
                AlertasPresupuesto = alertas,
                RecordatoriosPendientes = await _context.Recordatorios
                    .Where(x => x.UsuarioId == user.Id && !x.Completado)
                    .OrderBy(x => x.FechaRecordatorio)
                    .Take(5)
                    .ToListAsync(),
                Metas = await _context.MetasAhorro
                    .Where(x => x.UsuarioId == user.Id)
                    .OrderBy(x => x.FechaObjetivo)
                    .Take(4)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}
