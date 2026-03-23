// Controllers/AccountController.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BancaUT.Models;
using BancaUT.Models.ViewModels;

namespace BancaUT.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        // ASP.NET inyecta estos servicios automáticamente gracias
        // a lo que configuramos en Program.cs
        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // ══════════════════════════════════════════
        // REGISTRO
        // ══════════════════════════════════════════

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                FechaRegistro = DateTime.UtcNow
            };

            // CreateAsync encripta la contraseña automáticamente con PBKDF2
            // NUNCA guardamos la contraseña en texto plano
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("Nuevo usuario registrado: {Email}", model.Email);
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["Exito"] = $"¡Bienvenido a BancaUT, {user.Nombre}!";
                return RedirectToAction("Index", "Dashboard");
            }

            // Mostrar errores: email duplicado, contraseña débil, etc.
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // ══════════════════════════════════════════
        // INICIO DE SESIÓN
        // ══════════════════════════════════════════

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,       // true = cookie persiste al cerrar browser
                lockoutOnFailure: true  // activa bloqueo tras 5 intentos
            );

            if (result.Succeeded)
            {
                _logger.LogInformation("Login exitoso: {Email}", model.Email);

                // Validar que returnUrl sea local (evitar Open Redirect attacks)
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Dashboard");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty,
                    "Cuenta bloqueada temporalmente. Intenta de nuevo en 15 minutos.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty,
                "Correo o contraseña incorrectos.");
            return View(model);
        }

        // ══════════════════════════════════════════
        // CERRAR SESIÓN
        // ══════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Usuario cerró sesión");
            return RedirectToAction("Login");
        }

        // ══════════════════════════════════════════
        // RECUPERAR CONTRASEÑA — Paso 1
        // ══════════════════════════════════════════

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            // ⚠️ Seguridad: siempre redirigir aunque el email no exista.
            // Si mostramos error "ese email no existe", un atacante
            // puede descubrir qué cuentas existen en el sistema.
            if (user == null)
                return RedirectToAction("ForgotPasswordConfirmation");

            // Generar token seguro firmado con HMAC (expira automáticamente)
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resetLink = Url.Action(
                "ResetPassword", "Account",
                new { userId = user.Id, token = token },
                protocol: HttpContext.Request.Scheme
            );

            // 📧 EN PRODUCCIÓN: enviar resetLink por email (SendGrid, SMTP, etc.)
            // Por ahora lo guardamos en TempData para poder probarlo
            TempData["ResetLink"] = resetLink;
            _logger.LogInformation("Reset link generado para: {Email}", model.Email);

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation() => View();

        // ══════════════════════════════════════════
        // RECUPERAR CONTRASEÑA — Paso 2
        // ══════════════════════════════════════════

        [HttpGet]
        public IActionResult ResetPassword(string? userId, string? token)
        {
            if (userId == null || token == null)
                return BadRequest("Enlace de recuperación inválido o expirado.");

            return View(new ResetPasswordViewModel { UserId = userId, Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
                return RedirectToAction("ResetPasswordConfirmation");

            // Valida el token Y encripta la nueva contraseña en un solo paso
            var result = await _userManager.ResetPasswordAsync(
                user, model.Token, model.Password);

            if (result.Succeeded)
                return RedirectToAction("ResetPasswordConfirmation");

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation() => View();

        // ══════════════════════════════════════════
        // EDITAR PERFIL
        // ══════════════════════════════════════════

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            return View(new EditProfileViewModel
            {
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                PhoneNumber = user.PhoneNumber
            });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Actualizar datos básicos
            user.Nombre = model.Nombre;
            user.Apellido = model.Apellido;
            user.PhoneNumber = model.PhoneNumber;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            // Cambio de contraseña (solo si el usuario llenó esos campos)
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword",
                        "Debes ingresar tu contraseña actual para cambiarla.");
                    return View(model);
                }

                var passResult = await _userManager.ChangePasswordAsync(
                    user, model.CurrentPassword, model.NewPassword);

                if (!passResult.Succeeded)
                {
                    foreach (var error in passResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return View(model);
                }

                // Refrescar cookie para evitar que se cierre la sesión
                await _signInManager.RefreshSignInAsync(user);
            }

            TempData["Exito"] = "Perfil actualizado correctamente.";
            return RedirectToAction("EditProfile");
        }

        // ══════════════════════════════════════════
        // ACCESO DENEGADO
        // ══════════════════════════════════════════

        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}