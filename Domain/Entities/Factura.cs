using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Interfaces;

namespace Domain.Entities
{
    public class Factura
    {
        [Key]
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public DateTime Fecha { get; set; }

        [Required, MaxLength(15)]
        public string Numero { get; set; } = string.Empty; // Formato 001-001-000000001
        public decimal Subtotal { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = "Emitida";

        // --- CAMPOS PARA INTEGRACIÓN SRI ---
        [Required, MaxLength(49)]
        public string ClaveAcceso { get; set; } = string.Empty;
        public string EstadoSRI { get; set; } = "PENDIENTE";
        public string? MotivoRechazo { get; set; }
        public string? XmlComprobante { get; set; }
        public string? XmlRecepcion { get; set; }

        // --- PROPIEDADES DE NAVEGACIÓN ---
        // CRÍTICO: Esta propiedad faltaba o estaba mal nombrada.
        public Cliente Cliente { get; set; } = null!;
        public ICollection<DetalleFactura> DetalleFacturas { get; set; } = new List<DetalleFactura>();

        // --- SOFT DELETE ---
        public bool IsDeleted { get; set; }
    }
}