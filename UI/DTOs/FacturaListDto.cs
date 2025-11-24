namespace UI.DTOs
{
    public class FacturaListDto
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
        public string EstadoSRI { get; set; } = string.Empty;
    }
}
