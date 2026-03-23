using BancaUT.Data;
using BancaUT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BancaUT.Controllers
{
    [Authorize]
    public class MetasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MetasController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var metas = await _context.MetasAhorro
                .Where(x => x.UsuarioId == user.Id)
                .OrderBy(x => x.FechaObjetivo)
                .ToListAsync();

            return View(metas);
        }

        public IActionResult Create()
        {
            return View(new MetaAhorro
            {
                FechaInicio = DateTime.Today,
                FechaObjetivo = DateTime.Today.AddMonths(6)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MetaAhorro meta)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            meta.UsuarioId = user.Id;
            ModelState.Remove(nameof(MetaAhorro.UsuarioId));

            if (meta.FechaObjetivo.Date < meta.FechaInicio.Date)
                ModelState.AddModelError(nameof(MetaAhorro.FechaObjetivo), "La fecha objetivo no puede ser anterior a la fecha de inicio.");

            if (!ModelState.IsValid)
                return View(meta);

            _context.MetasAhorro.Add(meta);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Meta de ahorro creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarBono(int id, decimal bono)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var meta = await _context.MetasAhorro.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == user.Id);
            if (meta == null) return NotFound();

            if (bono <= 0)
            {
                TempData["Error"] = "El bono debe ser mayor que cero.";
                return RedirectToAction(nameof(Index));
            }

            meta.MontoActual += bono;
            await _context.SaveChangesAsync();

            TempData["Exito"] = "Bono agregado correctamente a la meta.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var meta = await _context.MetasAhorro.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == user.Id);
            if (meta == null) return NotFound();

            _context.MetasAhorro.Remove(meta);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Meta eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
