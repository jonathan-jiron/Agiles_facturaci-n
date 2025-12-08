using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class PagoFactura
{
    public int Id { get; set; }
    
    public int FacturaId { get; set; }
    public Factura? Factura { get; set; }
    
    [Required, MaxLength(50)]
    public string FormaPago { get; set; } = string.Empty; // Efectivo, Transferencia, etc.
    
    [Required]
    public decimal Monto { get; set; }
    
    [MaxLength(50)]
    public string? NumeroComprobante { get; set; } // Para transferencias, cheques, etc.
    
    public int Orden { get; set; } // Para mantener el orden de los pagos
}

