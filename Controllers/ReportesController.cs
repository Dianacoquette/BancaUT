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
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> BalanceGeneral()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var movimientos = await _context.Movimientos
                .Where(x => x.UsuarioId == user.Id)
                .Include(x => x.Categoria)
                .OrderByDescending(x => x.Fecha)
                .ToListAsync();

            var ingresos = movimientos.Where(x => x.Tipo == "Ingreso").Sum(x => x.Monto);
            var egresos = movimientos.Where(x => x.Tipo == "Egreso").Sum(x => x.Monto);
            var saldo = ingresos - egresos;

            var metas = await _context.MetasAhorro
                .Where(x => x.UsuarioId == user.Id)
                .ToListAsync();

            var model = new BalanceGeneralViewModel
            {
                TotalIngresos = ingresos,
                TotalEgresos = egresos,
                SaldoDisponible = saldo,
                TotalAhorradoMetas = metas.Sum(x => x.MontoActual),
                TotalPendienteMetas = metas.Sum(x => Math.Max(x.MontoObjetivo - x.MontoActual, 0m)),
                PatrimonioEstimado = saldo + metas.Sum(x => x.MontoActual),
                IngresosPorCategoria = BuildCategorySummary(movimientos.Where(x => x.Tipo == "Ingreso").ToList(), ingresos),
                EgresosPorCategoria = BuildCategorySummary(movimientos.Where(x => x.Tipo == "Egreso").ToList(), egresos),
                UltimosMovimientos = movimientos.Take(10).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> Mensual(int? mes, int? anio)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var today = DateTime.Today;
            var selectedMonth = mes ?? today.Month;
            var selectedYear = anio ?? today.Year;

            var movimientos = await _context.Movimientos
                .Where(x => x.UsuarioId == user.Id && x.Fecha.Month == selectedMonth && x.Fecha.Year == selectedYear)
                .Include(x => x.Categoria)
                .OrderByDescending(x => x.Fecha)
                .ThenByDescending(x => x.Id)
                .ToListAsync();

            var ingresos = movimientos.Where(x => x.Tipo == "Ingreso").Sum(x => x.Monto);
            var egresos = movimientos.Where(x => x.Tipo == "Egreso").Sum(x => x.Monto);

            var presupuestos = await _context.Presupuestos
                .Include(x => x.Categoria)
                .Where(x => x.UsuarioId == user.Id && x.Mes == selectedMonth && x.Anio == selectedYear)
                .ToListAsync();

            var model = new ReporteMensualViewModel
            {
                Mes = selectedMonth,
                Anio = selectedYear,
                TotalIngresos = ingresos,
                TotalEgresos = egresos,
                BalanceMes = ingresos - egresos,
                IngresosPorCategoria = movimientos
                    .Where(x => x.Tipo == "Ingreso" && x.Categoria != null)
                    .GroupBy(x => x.Categoria!.Nombre)
                    .Select(g => new ReporteCategoriaItem { Categoria = g.Key, Total = g.Sum(x => x.Monto) })
                    .OrderByDescending(x => x.Total)
                    .ToList(),
                EgresosPorCategoria = movimientos
                    .Where(x => x.Tipo == "Egreso" && x.Categoria != null)
                    .GroupBy(x => x.Categoria!.Nombre)
                    .Select(g => new ReporteCategoriaItem { Categoria = g.Key, Total = g.Sum(x => x.Monto) })
                    .OrderByDescending(x => x.Total)
                    .ToList(),
                Presupuestos = presupuestos.Select(p =>
                {
                    var gastado = movimientos.Where(m => m.Tipo == "Egreso" && m.CategoriaId == p.CategoriaId).Sum(m => m.Monto);
                    var porcentaje = p.MontoLimite > 0 ? Math.Round((gastado / p.MontoLimite) * 100, 1) : 0;
                    return new PresupuestoMensualEstadoItem
                    {
                        Categoria = p.Categoria?.Nombre ?? "Sin categoría",
                        Limite = p.MontoLimite,
                        Gastado = gastado,
                        PorcentajeUsado = porcentaje,
                        Excedido = gastado > p.MontoLimite
                    };
                }).OrderByDescending(x => x.PorcentajeUsado).ToList(),
                Movimientos = movimientos
            };

            return View(model);
        }

        private static List<BalanceCategoriaItem> BuildCategorySummary(List<Movimiento> movimientos, decimal total)
        {
            return movimientos
                .Where(x => x.Categoria != null)
                .GroupBy(x => x.Categoria!.Nombre)
                .Select(g => new BalanceCategoriaItem
                {
                    Nombre = g.Key,
                    Total = g.Sum(x => x.Monto),
                    Porcentaje = total > 0 ? Math.Round((g.Sum(x => x.Monto) / total) * 100, 1) : 0
                })
                .OrderByDescending(x => x.Total)
                .ToList();
        }
    }
}
