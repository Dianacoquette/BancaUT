using BancaUT.Models;

namespace BancaUT.Models.ViewModels
{
    public class BalanceGeneralViewModel
    {
        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal SaldoDisponible { get; set; }
        public decimal TotalAhorradoMetas { get; set; }
        public decimal TotalPendienteMetas { get; set; }
        public decimal PatrimonioEstimado { get; set; }
        public List<BalanceCategoriaItem> IngresosPorCategoria { get; set; } = new();
        public List<BalanceCategoriaItem> EgresosPorCategoria { get; set; } = new();
        public List<Movimiento> UltimosMovimientos { get; set; } = new();
    }

    public class BalanceCategoriaItem
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal Porcentaje { get; set; }
    }
}
