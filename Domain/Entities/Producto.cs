namespace Domain.Entities
{
    public class Producto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }

        // Nuevo: precio de venta y si aplica IVA
        public decimal PrecioVenta { get; set; }
        public bool AplicaIva { get; set; }

        // Relación con Lotes
        public List<Lote> Lotes { get; set; } = new List<Lote>();

        // Soft delete
        public bool IsDeleted { get; set; } = false;

        public byte[]? RowVersion { get; set; }
    }
}
