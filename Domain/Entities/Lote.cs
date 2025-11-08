namespace Domain.Entities
{
    public class Lote
    {
        public int Id { get; set; }
        public string CodigoLote { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public DateTime FechaIngreso { get; set; } // 👈 Para FIFO
        public int ProductoId { get; set; }
        public Producto Producto { get; set; } = null!;
        public decimal Precio { get; set; } // 👈 Nuevo campo para precio por lote
    }
}
