using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BancaUT.Data;
using BancaUT.Models;

namespace BancaUT.Controllers
{
    [Authorize]
    public class CategoriasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CategoriasController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var categorias = await _context.Categorias
                .Where(x => x.UsuarioId == user.Id)
                .OrderBy(x => x.Tipo)
                .ThenBy(x => x.Nombre)
                .ToListAsync();

            return View(categorias);
        }

        public IActionResult Create()
        {
            return View(new Categoria());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Categoria categoria)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            categoria.UsuarioId = user.Id;
            ModelState.Remove(nameof(Categoria.UsuarioId));

            if (!ModelState.IsValid)
                return View(categoria);

            var existe = await _context.Categorias.AnyAsync(x => x.UsuarioId == user.Id && x.Nombre == categoria.Nombre && x.Tipo == categoria.Tipo);
            if (existe)
            {
                ModelState.AddModelError(nameof(Categoria.Nombre), "Ya existe una categoría con ese nombre y tipo.");
                return View(categoria);
            }

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Categoría creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var categoria = await _context.Categorias.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == user.Id);
            if (categoria == null) return NotFound();

            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Categoria categoria)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            if (id != categoria.Id) return NotFound();

            var original = await _context.Categorias.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == user.Id);
            if (original == null) return NotFound();

            if (!ModelState.IsValid)
                return View(categoria);

            original.Nombre = categoria.Nombre;
            original.Tipo = categoria.Tipo;

            await _context.SaveChangesAsync();
            TempData["Exito"] = "Categoría actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var categoria = await _context.Categorias.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == user.Id);
            if (categoria == null) return NotFound();

            var tieneDependencias = await _context.Movimientos.AnyAsync(x => x.CategoriaId == id) || await _context.Presupuestos.AnyAsync(x => x.CategoriaId == id);
            if (tieneDependencias)
            {
                TempData["Error"] = "No puedes eliminar esta categoría porque ya está siendo usada en movimientos o presupuestos.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Categoría eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
