// Models/Categoria.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BancaUT.Models
{
    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        // FK → Usuario dueño de esta categoría
        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        // "Ingreso" o "Egreso"
        [Required(ErrorMessage = "El tipo es obligatorio")]
        [StringLength(20)]
        public string Tipo { get; set; } = string.Empty;

        // ── Navegación ──────────────────────────
        [ForeignKey("UsuarioId")]
        public ApplicationUser? Usuario { get; set; }

        public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
        public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
    }
}