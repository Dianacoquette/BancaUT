// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BancaUT.Models
{
    // Heredamos de IdentityUser para no escribir todo desde cero.
    // IdentityUser ya incluye: Id, Email, PasswordHash, UserName,
    // PhoneNumber, LockoutEnd, AccessFailedCount, etc.
    // Solo agregamos los campos EXTRA que necesita BancaUT.
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Apellido { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        // Nombre completo calculado — no se guarda en BD
        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
}