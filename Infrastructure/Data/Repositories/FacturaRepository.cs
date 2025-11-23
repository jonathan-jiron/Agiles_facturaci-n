using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class FacturaRepository : IFacturaRepository
{
    private readonly ApplicationDbContext _db;
    public FacturaRepository(ApplicationDbContext db) => _db = db;

    public async Task<Factura> AgregarAsync(Factura factura)
    {
        _db.Facturas.Add(factura);
        await _db.SaveChangesAsync();
        return factura;
    }

    public async Task<Factura?> ObtenerPorIdAsync(int id)
    {
        return await _db.Facturas
            .Include(f => f.Detalles)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Factura?> ObtenerPorNumeroAsync(string numero)
    {
        return await _db.Facturas
            .Include(f => f.Detalles)
            .FirstOrDefaultAsync(f => f.Numero == numero);
    }

    public async Task<List<Factura>> ListarPorClienteAsync(int clienteId)
    {
        return await _db.Facturas
            .Where(f => f.ClienteId == clienteId)
            .Include(f => f.Detalles)
            .ToListAsync();
    }

    public async Task<List<Factura>> ListarAsync()
    {
        return await _db.Facturas
            .Include(f => f.Detalles)
            .OrderByDescending(f => f.Fecha)
            .ToListAsync();
    }

    public async Task UpdateAsync(Factura factura)
    {
        _db.Facturas.Update(factura);
        await _db.SaveChangesAsync();
    }

    public async Task<Cliente?> GetClienteById(int clienteId)
    {
        return await _db.Clientes.FirstOrDefaultAsync(c => c.Id == clienteId);
    }
}
