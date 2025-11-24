using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class DetalleFacturaRepository : IDetalleFacturaRepository
{
    private readonly ApplicationDbContext _db;
    public DetalleFacturaRepository(ApplicationDbContext db) => _db = db;

    public async Task<DetalleFactura> AgregarAsync(DetalleFactura detalle)
    {
        _db.DetallesFactura.Add(detalle);
        await _db.SaveChangesAsync();
        return detalle;
    }

    public async Task<List<DetalleFactura>> ListarPorFacturaAsync(int facturaId)
    {
        return await _db.DetallesFactura
            .Where(d => d.FacturaId == facturaId)
            .Include(d => d.Producto)
            .ToListAsync();
    }
}
