// Models/MetaAhorro.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BancaUT.Models
{
    public class MetaAhorro
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de la meta es obligatorio")]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto objetivo es obligatorio")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal MontoObjetivo { get; set; }

        // Cuánto se ha ahorrado hasta ahora (inicia en 0)
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoActual { get; set; } = 0;

        [Required]
        public DateTime FechaInicio { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "La fecha objetivo es obligatoria")]
        public DateTime FechaObjetivo { get; set; }

        // Propiedad calculada — NO se guarda en BD
        [NotMapped]
        public decimal PorcentajeAvance =>
            MontoObjetivo > 0
            ? Math.Min(Math.Round((MontoActual / MontoObjetivo) * 100, 1), 100)
            : 0;

        // ── Navegación ──────────────────────────
        [ForeignKey("UsuarioId")]
        public ApplicationUser? Usuario { get; set; }
    }
}