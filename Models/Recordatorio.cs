// Models/Recordatorio.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BancaUT.Models
{
    public class Recordatorio
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(150)]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime FechaRecordatorio { get; set; }

        // false = pendiente, true = completado
        public bool Completado { get; set; } = false;

        // ── Navegación ──────────────────────────
        [ForeignKey("UsuarioId")]
        public ApplicationUser? Usuario { get; set; }
    }
}