namespace BancaUT.Models.ViewModels
{
    public class PresupuestoIndexViewModel
    {
        public int Id { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public decimal MontoLimite { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public decimal Gastado { get; set; }
        public decimal PorcentajeUsado { get; set; }
        public bool Excedido { get; set; }
        public decimal Disponible => Math.Max(MontoLimite - Gastado, 0m);
    }
}
