using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class FacturaService
{
    private readonly IFacturaRepository _facturaRepo;
    private readonly IDetalleFacturaRepository _detalleRepo;
    private readonly Application.Interfaces.IProductLookup _productLookup;
    private readonly Application.Interfaces.ILoteAllocator _loteAllocator;

    public FacturaService(
        IFacturaRepository facturaRepo, 
        IDetalleFacturaRepository detalleRepo, 
        Application.Interfaces.IProductLookup productLookup,
        Application.Interfaces.ILoteAllocator loteAllocator)
    {
        _facturaRepo = facturaRepo;
        _detalleRepo = detalleRepo;
        _productLookup = productLookup;
        _loteAllocator = loteAllocator;
    }

    public async Task<Factura> CrearFacturaAsync(FacturaCreateDto dto)
    {
        var numero = "FAC-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var factura = new Factura
        {
            Numero = numero,
            Fecha = DateTime.UtcNow,
            ClienteId = dto.ClienteId
        };

        decimal subtotal = 0m;
        foreach (var d in dto.Detalles)
        {
            // Asignar lotes por FIFO y decrementar stock
            var allocations = await _loteAllocator.AllocateAsync(d.ProductoId, d.Cantidad);
            
            // Obtener precio unitario desde la fuente de verdad (producto)
            var precio = await _productLookup.GetUnitPriceAsync(d.ProductoId);
            
            // Crear un detalle por cada lote asignado (o uno consolidado, según negocio)
            // Opción: consolidar todos los lotes en un solo detalle
            var detalle = new DetalleFactura
            {
                ProductoId = d.ProductoId,
                // LoteId se asigna al primer lote consumido (o null si múltiples)
                LoteId = allocations.Count == 1 ? allocations[0].LoteId : null,
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

    public Task<List<Factura>> ListarAsync() => _facturaRepo.ListarAsync();
}
