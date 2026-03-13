// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BancaUT.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Apellido { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public string NombreCompleto => $"{Nombre} {Apellido}";

        // ── Navegación — todas las relaciones del usuario ──
        public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
        public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
        public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
        public ICollection<MetaAhorro> MetasAhorro { get; set; } = new List<MetaAhorro>();
        public ICollection<Recordatorio> Recordatorios { get; set; } = new List<Recordatorio>();
    }
}