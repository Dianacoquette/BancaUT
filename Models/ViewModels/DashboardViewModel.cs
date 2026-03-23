using BancaUT.Models;

namespace BancaUT.Models.ViewModels
{
    public class DashboardViewModel
    {
        public decimal Ingresos { get; set; }
        public decimal Gastos { get; set; }
        public decimal Saldo { get; set; }
        public decimal AhorradoMetas { get; set; }
        public List<GastoCategoriaItem> GastosPorCategoria { get; set; } = new();
        public List<Movimiento> MovimientosRecientes { get; set; } = new();
        public List<PresupuestoAlertaItem> AlertasPresupuesto { get; set; } = new();
        public List<Recordatorio> RecordatoriosPendientes { get; set; } = new();
        public List<MetaAhorro> Metas { get; set; } = new();
    }

    public class GastoCategoriaItem
    {
        public string Categoria { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class PresupuestoAlertaItem
    {
        public int PresupuestoId { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public decimal Limite { get; set; }
        public decimal Gastado { get; set; }
        public decimal PorcentajeUsado { get; set; }
        public bool Excedido => Gastado > Limite;
        public decimal Disponible => Math.Max(Limite - Gastado, 0m);
        public string NivelAlerta => Excedido ? "Excedido" : PorcentajeUsado >= 80 ? "Alerta" : "Normal";
    }
}
