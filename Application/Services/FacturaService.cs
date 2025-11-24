using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class FacturaService
{
    private readonly IFacturaRepository _facturaRepo;
    private readonly IDetalleFacturaRepository _detalleRepo;

        private readonly Application.Interfaces.IProductLookup _productLookup;

        public FacturaService(IFacturaRepository facturaRepo, IDetalleFacturaRepository detalleRepo, Application.Interfaces.IProductLookup productLookup)
        {
            _facturaRepo = facturaRepo;
            _detalleRepo = detalleRepo;
            _productLookup = productLookup;
        }

    public async Task<Factura> CrearFacturaAsync(FacturaCreateDto dto)
    {
        var numero = "001-001-" + DateTime.UtcNow.ToString("HHmmssfff");
        var factura = new Factura
        {
            Numero = numero,
            Fecha = DateTime.UtcNow,
            ClienteId = dto.ClienteId
        };

        decimal subtotal = 0m;
            foreach (var d in dto.Detalles)
            {
                // Obtener precio unitario desde la fuente de verdad (producto)
                var precio = await _productLookup.GetUnitPriceAsync(d.ProductoId);
                var detalle = new DetalleFactura
                {
                    ProductoId = d.ProductoId,
                    // Lote is assigned by FIFO allocation at inventory layer; do not set here
                    Cantidad = d.Cantidad,
                    PrecioUnitario = precio
                };
                detalle.Total = detalle.PrecioUnitario * detalle.Cantidad;
                detalle.Iva = Math.Round(detalle.Total * 0.12m, 2);
                subtotal += detalle.Total;
                factura.Detalles.Add(detalle);
            }

        factura.Subtotal = subtotal;
        factura.Iva = Math.Round(subtotal * 0.12m, 2);
        factura.Total = factura.Subtotal + factura.Iva;

        var created = await _facturaRepo.AgregarAsync(factura);
        return created;
    }

    public Task<Factura?> ObtenerPorIdAsync(int id) => _facturaRepo.ObtenerPorIdAsync(id);

    public async Task<List<FacturaDto>> ListarAsync()
    {
        var facturas = await _facturaRepo.ListarAsync();
        return facturas.Select(f => new FacturaDto
        {
            Id = f.Id,
            Numero = f.Numero,
            Fecha = f.Fecha,
            ClienteId = f.ClienteId,
            ClienteNombre = f.Cliente?.NombreRazonSocial,
            Subtotal = f.Subtotal,
            Iva = f.Iva,
            Total = f.Total,
            EstadoSRI = f.EstadoSRI
        }).ToList();
    }
}
