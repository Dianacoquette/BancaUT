using BancaUT.Models;

namespace BancaUT.Models.ViewModels
{
    public class RecordatoriosIndexViewModel
    {
        public List<Recordatorio> Pendientes { get; set; } = new();
        public List<Recordatorio> Completados { get; set; } = new();
    }
}
