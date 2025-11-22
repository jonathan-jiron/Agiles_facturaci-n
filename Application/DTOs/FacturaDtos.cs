using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class DetalleFacturaCreateDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}

public class DetalleFacturaDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}

public class FacturaCreateDto
{
    [Required]
    public int ClienteId { get; set; }

    public List<DetalleFacturaCreateDto> Detalles { get; set; } = new List<DetalleFacturaCreateDto>();
}

public class FacturaDto
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public int ClienteId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public List<DetalleFacturaDto> Detalles { get; set; } = new List<DetalleFacturaDto>();
}
