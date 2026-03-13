// Models/Movimiento.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BancaUT.Models
{
    public class Movimiento
    {
        [Key]
        public int Id { get; set; }

        // FK → Usuario (para consultas rápidas sin pasar por categoría)
        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        // FK → Categoría del movimiento
        [Required(ErrorMessage = "La categoría es obligatoria")]
        public int CategoriaId { get; set; }

        // "Ingreso" o "Egreso"
        [Required(ErrorMessage = "El tipo es obligatorio")]
        [StringLength(20)]
        public string Tipo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        [StringLength(255)]
        public string? Descripcion { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        // ── Navegación ──────────────────────────
        [ForeignKey("UsuarioId")]
        public ApplicationUser? Usuario { get; set; }

        [ForeignKey("CategoriaId")]
        public Categoria? Categoria { get; set; }
    }
}