using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class FacturaCreateDto
{
    public int ClienteId { get; set; }
    public DateTime Fecha { get; set; }
    public string Establecimiento { get; set; } = "";
    public string PuntoEmision { get; set; } = "";
    public string FormaPago { get; set; } = ""; // Mantener por compatibilidad hacia atrás
    public string Observaciones { get; set; } = "";
    public string NumeroComprobante { get; set; } = ""; // Mantener por compatibilidad
    public List<DetalleFacturaCreateDto> Detalles { get; set; } = new();
    public List<PagoFacturaCreateDto> Pagos { get; set; } = new(); // Nuevo: lista de pagos múltiples
}

public class PagoFacturaCreateDto
{
    public string FormaPago { get; set; } = "";
    public decimal Monto { get; set; }
    public string? NumeroComprobante { get; set; }
    public int Orden { get; set; }
}

public class DetalleFacturaCreateDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; }
    public decimal IvaLinea { get; set; }
    public decimal IceLinea { get; set; }
    public decimal IrbpnrLinea { get; set; }
    public int LoteId { get; set; }
}

public class FacturaDto
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public int ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public string? ClienteIdentificacion { get; set; }
    public string? ClienteEmail { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public List<DetalleFacturaDto> Detalles { get; set; } = new();
}

public class DetalleFacturaDto
{
    public int ProductoId { get; set; }
    public string? ProductoNombre { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; }
    public decimal IvaLinea { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
}

public class FacturaListDto
{
    public int Id { get; set; }
    public string Numero { get; set; } = "";
    public DateTime Fecha { get; set; }
    public string ClienteNombre { get; set; } = "";
    public decimal Total { get; set; }
    public string Estado { get; set; } = "";
    public string? EstadoSri { get; set; }
    public string? ClaveAcceso { get; set; }
}

// Puedes poner esto en ProductoSelector.razor.cs o donde corresponda
public class ProductoDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public decimal PrecioVenta { get; set; }
    public bool AplicaIva { get; set; }
    public int Stock { get; set; }
    public bool TieneIva => AplicaIva;
}
