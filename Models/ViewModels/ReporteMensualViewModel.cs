namespace BancaUT.Models.ViewModels
{
    public class ReporteMensualViewModel
    {
        public int Mes { get; set; }
        public int Anio { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal BalanceMes { get; set; }
        public List<ReporteCategoriaItem> IngresosPorCategoria { get; set; } = new();
        public List<ReporteCategoriaItem> EgresosPorCategoria { get; set; } = new();
        public List<PresupuestoMensualEstadoItem> Presupuestos { get; set; } = new();
        public List<BancaUT.Models.Movimiento> Movimientos { get; set; } = new();
        public string NombreMes => new DateTime(Anio, Mes, 1).ToString("MMMM");
    }

    public class ReporteCategoriaItem
    {
        public string Categoria { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class PresupuestoMensualEstadoItem
    {
        public string Categoria { get; set; } = string.Empty;
        public decimal Limite { get; set; }
        public decimal Gastado { get; set; }
        public decimal PorcentajeUsado { get; set; }
        public bool Excedido { get; set; }
    }
}
