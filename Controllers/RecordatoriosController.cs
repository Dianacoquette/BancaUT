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
    public class RecordatoriosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RecordatoriosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var items = await _context.Recordatorios
                .Where(x => x.UsuarioId == user.Id)
                .OrderBy(x => x.Completado)
                .ThenBy(x => x.FechaRecordatorio)
                .ToListAsync();

            var model = new RecordatoriosIndexViewModel
            {
                Pendientes = items.Where(x => !x.Completado).ToList(),
                Completados = items.Where(x => x.Completado).ToList()
            };

            return View(model);
        }

        public IActionResult Create()
        {
            return View(new Recordatorio { FechaRecordatorio = DateTime.Today.AddDays(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Recordatorio model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            model.UsuarioId = user.Id;
            ModelState.Remove(nameof(Recordatorio.UsuarioId));

            if (!ModelState.IsValid)
                return View(model);

            _context.Recordatorios.Add(model);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Recordatorio creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var item = await _context.Recordatorios.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == user.Id);
            if (item == null) return NotFound();

            item.Completado = !item.Completado;
            await _context.SaveChangesAsync();

            TempData["Exito"] = item.Completado ? "Recordatorio marcado como pagado/completado." : "Recordatorio marcado como pendiente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var item = await _context.Recordatorios.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == user.Id);
            if (item == null) return NotFound();

            _context.Recordatorios.Remove(item);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Recordatorio eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
