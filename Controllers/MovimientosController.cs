using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BancaUT.Data;
using BancaUT.Models;

namespace BancaUT.Controllers
{
    [Authorize]
    public class MovimientosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MovimientosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? tipo, int? mes, int? anio, string? texto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var query = _context.Movimientos
                .Include(x => x.Categoria)
                .Where(x => x.UsuarioId == user.Id)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tipo))
                query = query.Where(x => x.Tipo == tipo);

            if (mes.HasValue)
                query = query.Where(x => x.Fecha.Month == mes.Value);

            if (anio.HasValue)
                query = query.Where(x => x.Fecha.Year == anio.Value);

            if (!string.IsNullOrWhiteSpace(texto))
                query = query.Where(x => (x.Descripcion ?? "").Contains(texto) || (x.Categoria != null && x.Categoria.Nombre.Contains(texto)));

            var movimientos = await query
                .OrderByDescending(x => x.Fecha)
                .ThenByDescending(x => x.Id)
                .ToListAsync();

            ViewBag.Tipo = tipo;
            ViewBag.Mes = mes;
            ViewBag.Anio = anio;
            ViewBag.Texto = texto;
            ViewBag.IngresosFiltrados = movimientos.Where(x => x.Tipo == "Ingreso").Sum(x => x.Monto);
            ViewBag.EgresosFiltrados = movimientos.Where(x => x.Tipo == "Egreso").Sum(x => x.Monto);

            return View(movimientos);
        }

        public async Task<IActionResult> Create(string tipo = "Egreso")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            await CargarCategorias(user.Id, tipo);
            ViewBag.SaldoActual = await ObtenerSaldoActual(user.Id);

            return View(new Movimiento
            {
                Tipo = tipo,
                Fecha = DateTime.Today
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movimiento movimiento)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            movimiento.UsuarioId = user.Id;
            ModelState.Remove(nameof(Movimiento.UsuarioId));

            var categoriaValida = await _context.Categorias.AnyAsync(x => x.Id == movimiento.CategoriaId && x.UsuarioId == user.Id && x.Tipo == movimiento.Tipo);
            if (!categoriaValida)
                ModelState.AddModelError(nameof(Movimiento.CategoriaId), "Selecciona una categoría válida para el tipo de movimiento.");

            var saldoActual = await ObtenerSaldoActual(user.Id);
            if (movimiento.Tipo == "Egreso" && movimiento.Monto > saldoActual)
                ModelState.AddModelError(nameof(Movimiento.Monto), $"No puedes gastar más de lo que tienes disponible. Saldo actual: ${saldoActual:N2}.");

            if (!ModelState.IsValid)
            {
                await CargarCategorias(user.Id, movimiento.Tipo, movimiento.CategoriaId);
                ViewBag.SaldoActual = saldoActual;
                return View(movimiento);
            }

            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Movimiento registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var mov = await _context.Movimientos.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == user.Id);
            if (mov == null) return NotFound();

            await CargarCategorias(user.Id, mov.Tipo, mov.CategoriaId);
            ViewBag.SaldoDisponibleEdicion = await ObtenerSaldoDisponibleParaEdicion(user.Id, mov);
            return View(mov);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movimiento movimiento)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            if (id != movimiento.Id) return NotFound();

            var original = await _context.Movimientos.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == user.Id);
            if (original == null) return NotFound();

            movimiento.UsuarioId = user.Id;
            ModelState.Remove(nameof(Movimiento.UsuarioId));

            var categoriaValida = await _context.Categorias.AnyAsync(x => x.Id == movimiento.CategoriaId && x.UsuarioId == user.Id && x.Tipo == movimiento.Tipo);
            if (!categoriaValida)
                ModelState.AddModelError(nameof(Movimiento.CategoriaId), "Selecciona una categoría válida para el tipo de movimiento.");

            var saldoDisponibleEdicion = await ObtenerSaldoDisponibleParaEdicion(user.Id, original);
            if (movimiento.Tipo == "Egreso" && movimiento.Monto > saldoDisponibleEdicion)
                ModelState.AddModelError(nameof(Movimiento.Monto), $"No puedes registrar un gasto mayor al saldo disponible. Disponible para esta edición: ${saldoDisponibleEdicion:N2}.");

            if (!ModelState.IsValid)
            {
                await CargarCategorias(user.Id, movimiento.Tipo, movimiento.CategoriaId);
                ViewBag.SaldoDisponibleEdicion = saldoDisponibleEdicion;
                return View(movimiento);
            }

            original.CategoriaId = movimiento.CategoriaId;
            original.Tipo = movimiento.Tipo;
            original.Monto = movimiento.Monto;
            original.Descripcion = movimiento.Descripcion;
            original.Fecha = movimiento.Fecha;

            await _context.SaveChangesAsync();
            TempData["Exito"] = "Movimiento actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var mov = await _context.Movimientos.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == user.Id);
            if (mov == null) return NotFound();

            _context.Movimientos.Remove(mov);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Movimiento eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarCategorias(string usuarioId, string tipo, int? selectedId = null)
        {
            var categorias = await _context.Categorias
                .Where(x => x.UsuarioId == usuarioId && x.Tipo == tipo)
                .OrderBy(x => x.Nombre)
                .ToListAsync();

            ViewBag.Categorias = new SelectList(categorias, "Id", "Nombre", selectedId);
            ViewBag.Tipo = tipo;
        }

        private async Task<decimal> ObtenerSaldoActual(string usuarioId)
        {
            var ingresos = await _context.Movimientos.Where(x => x.UsuarioId == usuarioId && x.Tipo == "Ingreso").SumAsync(x => x.Monto);
            var egresos = await _context.Movimientos.Where(x => x.UsuarioId == usuarioId && x.Tipo == "Egreso").SumAsync(x => x.Monto);
            return ingresos - egresos;
        }

        private async Task<decimal> ObtenerSaldoDisponibleParaEdicion(string usuarioId, Movimiento movimientoOriginal)
        {
            var saldoBase = await ObtenerSaldoActual(usuarioId);

            return movimientoOriginal.Tipo == "Egreso"
                ? saldoBase + movimientoOriginal.Monto
                : saldoBase - movimientoOriginal.Monto;
        }
    }
}
