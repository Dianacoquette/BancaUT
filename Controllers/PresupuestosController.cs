using BancaUT.Data;
using BancaUT.Models;
using BancaUT.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BancaUT.Controllers
{
    [Authorize]
    public class PresupuestosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PresupuestosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var lista = await _context.Presupuestos
                .Include(x => x.Categoria)
                .Where(x => x.UsuarioId == user.Id)
                .OrderByDescending(x => x.Anio)
                .ThenByDescending(x => x.Mes)
                .ToListAsync();

            var movimientos = await _context.Movimientos
                .Where(x => x.UsuarioId == user.Id && x.Tipo == "Egreso")
                .ToListAsync();

            var model = lista.Select(item =>
            {
                var gastado = movimientos
                    .Where(m => m.CategoriaId == item.CategoriaId && m.Fecha.Month == item.Mes && m.Fecha.Year == item.Anio)
                    .Sum(m => m.Monto);
                var porcentaje = item.MontoLimite > 0 ? Math.Round((gastado / item.MontoLimite) * 100, 1) : 0;

                return new PresupuestoIndexViewModel
                {
                    Id = item.Id,
                    Categoria = item.Categoria?.Nombre ?? "Sin categoría",
                    MontoLimite = item.MontoLimite,
                    Mes = item.Mes,
                    Anio = item.Anio,
                    Gastado = gastado,
                    PorcentajeUsado = porcentaje,
                    Excedido = gastado > item.MontoLimite
                };
            }).ToList();

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            await CargarCategorias(user.Id);

            return View(new Presupuesto
            {
                Mes = DateTime.Today.Month,
                Anio = DateTime.Today.Year
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Presupuesto presupuesto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            presupuesto.UsuarioId = user.Id;
            ModelState.Remove(nameof(Presupuesto.UsuarioId));

            var categoriaValida = await _context.Categorias.AnyAsync(x => x.Id == presupuesto.CategoriaId && x.UsuarioId == user.Id && x.Tipo == "Egreso");
            if (!categoriaValida)
                ModelState.AddModelError(nameof(Presupuesto.CategoriaId), "Selecciona una categoría de gasto válida.");

            var duplicado = await _context.Presupuestos.AnyAsync(x => x.UsuarioId == user.Id && x.CategoriaId == presupuesto.CategoriaId && x.Mes == presupuesto.Mes && x.Anio == presupuesto.Anio);
            if (duplicado)
                ModelState.AddModelError(string.Empty, "Ya existe un presupuesto para esa categoría en el mes y año indicados.");

            if (!ModelState.IsValid)
            {
                await CargarCategorias(user.Id, presupuesto.CategoriaId);
                return View(presupuesto);
            }

            _context.Presupuestos.Add(presupuesto);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Presupuesto creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var p = await _context.Presupuestos.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == user.Id);
            if (p == null) return NotFound();

            _context.Presupuestos.Remove(p);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Presupuesto eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarCategorias(string usuarioId, int? selectedId = null)
        {
            var categorias = await _context.Categorias
                .Where(x => x.UsuarioId == usuarioId && x.Tipo == "Egreso")
                .OrderBy(x => x.Nombre)
                .ToListAsync();

            ViewBag.Categorias = new SelectList(categorias, "Id", "Nombre", selectedId);
        }
    }
}
