using Domain.Entities;

namespace Domain.Entities
{
    public class Lote
    {
        public int Id { get; set; }
        public string NumeroLote { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public DateTime FechaIngreso { get; set; } = DateTime.Now;
        public DateTime? FechaVencimiento { get; set; }

        // Relación con Producto
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }
    }
}