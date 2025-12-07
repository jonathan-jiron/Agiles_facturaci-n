public class FacturaConsultaDto
{
    public int Id { get; set; }
    public string Numero { get; set; } = "";
    public DateTime Fecha { get; set; }
    public string ClienteNombre { get; set; } = "";
    public string Estado { get; set; } = "";
    public bool Firmada { get; set; }
    public decimal Total { get; set; }
    public decimal Subtotal12 { get; set; }
    public decimal Subtotal0 { get; set; }
    public decimal DescuentoTotal { get; set; }
    public decimal Iva { get; set; }
    public decimal IceTotal { get; set; }
    public List<DetalleFacturaConsultaDto> Detalles { get; set; } = new();
}

public class DetalleFacturaConsultaDto
{
    public int Cantidad { get; set; }
    public string ProductoNombre { get; set; } = "";
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DescuentoPorcentaje { get; set; }
    // Elimina Unidad si no existe en Producto
}