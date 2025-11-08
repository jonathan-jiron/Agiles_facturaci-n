namespace Domain.Entities
{
    public class Lote
    {
        public int Id { get; set; }
        public string NumeroLote { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        
        // Relación con Producto
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }
    }
}
