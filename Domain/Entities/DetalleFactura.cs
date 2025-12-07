using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class DetalleFactura
{
    public int Id { get; set; }

    // Relación con Factura
    public int FacturaId { get; set; }
    public Factura? Factura { get; set; }

    // Relación con Producto/Lote
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public int? LoteId { get; set; }
    public Lote? Lote { get; set; }

    // Datos
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; } // <-- Agrega esto
    public decimal IvaLinea { get; set; }  // <-- Agrega esto
    public decimal Total { get; set; }
}

