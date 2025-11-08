namespace Domain.Entities
{
    public class Producto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        
        // Relación uno a muchos con Lotes
        public ICollection<Lote> Lotes { get; set; } = new List<Lote>();
    }
}
