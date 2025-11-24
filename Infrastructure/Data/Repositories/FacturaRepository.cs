using Domain.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Data.Repositories;

public class FacturaRepository : IFacturaRepository
{
    private readonly ApplicationDbContext _db;

    public FacturaRepository(ApplicationDbContext db) => _db = db;

    public async Task<Factura?> ObtenerFacturaParaSriAsync(int id)
    {
        // CARGA COMPLETA (EAGER LOADING) para el generador de XML:
        return await _db.Facturas
            .Include(f => f.Cliente)
            .Include(f => f.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Factura?> ObtenerPorIdAsync(int id)
    {
        return await _db.Facturas.FirstOrDefaultAsync(f => f.Id == id);
    }

    public Task UpdateAsync(Factura factura)
    {
        _db.Facturas.Update(factura);
        return _db.SaveChangesAsync();
    }

    public Task<Cliente?> GetClienteById(int clienteId)
    {
        return _db.Clientes.FirstOrDefaultAsync(c => c.Id == clienteId);
    }

    public Task<Factura> AgregarAsync(Factura factura)
    {
        _db.Facturas.Add(factura);
        return _db.SaveChangesAsync().ContinueWith(t => factura);
    }

    public Task<Factura?> ObtenerPorNumeroAsync(string numero)
    {
        return _db.Facturas.FirstOrDefaultAsync(f => f.Numero == numero);
    }

    public Task<List<Factura>> ListarPorClienteAsync(int clienteId)
    {
        return _db.Facturas.Where(f => f.ClienteId == clienteId).ToListAsync();
    }

    public Task<List<Factura>> ListarAsync()
    {
        return _db.Facturas
            .Include(f => f.Cliente)
            .ToListAsync();
    }
}