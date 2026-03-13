// Models/Presupuesto.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BancaUT.Models
{
    public class Presupuesto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoría es obligatoria")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "El monto límite es obligatorio")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal MontoLimite { get; set; }

        // Mes: 1–12
        [Required(ErrorMessage = "El mes es obligatorio")]
        [Range(1, 12, ErrorMessage = "Mes inválido")]
        public int Mes { get; set; }

        [Required(ErrorMessage = "El año es obligatorio")]
        [Range(2000, 2100, ErrorMessage = "Año inválido")]
        public int Anio { get; set; }

        // ── Navegación ──────────────────────────
        [ForeignKey("UsuarioId")]
        public ApplicationUser? Usuario { get; set; }

        [ForeignKey("CategoriaId")]
        public Categoria? Categoria { get; set; }
    }
}