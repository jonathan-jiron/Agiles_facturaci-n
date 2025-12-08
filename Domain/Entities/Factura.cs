using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;
using Domain.Enums;

public class Factura
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Numero { get; set; } = string.Empty;

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    // Relación con Cliente
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    // Detalles
    public List<DetalleFactura> Detalles { get; set; } = new List<DetalleFactura>();

    // Totales
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }

    // Estado del comprobante
    public EstadoFactura Estado { get; set; } = EstadoFactura.Emitida;

    // NUEVOS CAMPOS PARA SRI Y CONTIFICO
    [MaxLength(10)]
    public string Establecimiento { get; set; } = string.Empty;

    [MaxLength(10)]
    public string PuntoEmision { get; set; } = string.Empty;

    [MaxLength(30)]
    public string FormaPago { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Observaciones { get; set; } = string.Empty;

    public string? ClaveAcceso { get; set; }
    public string? SecuencialSri { get; set; }
    public string? NumeroAutorizacion { get; set; }
    public DateTime? FechaAutorizacion { get; set; }
    public string? EstadoSri { get; set; }
    public string? MensajesSri { get; set; }
    public string? XmlGenerado { get; set; }
    public string? XmlFirmado { get; set; }
    public string? XmlAutorizado { get; set; }
}
