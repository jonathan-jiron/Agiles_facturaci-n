using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

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
    public string Estado { get; set; } = "Emitida";

    // --- CAMPOS PARA INTEGRACIÓN SRI ---
    [Required, MaxLength(49)]
    public string ClaveAcceso { get; set; } = string.Empty;
    public string EstadoSRI { get; set; } = "PENDIENTE"; // PENDIENTE, PPR, AUT, NAT
    public string? XmlComprobante { get; set; } // XML Autorizado final
    public string? XmlRecepcion { get; set; } // XML Enviado a SRI (Firmado)
    public string? MotivoRechazo { get; set; }
}
