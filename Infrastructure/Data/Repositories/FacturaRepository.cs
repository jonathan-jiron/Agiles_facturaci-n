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
            .Include(f => f.Cliente)
            .OrderByDescending(f => f.Fecha)
            .ToListAsync();
    }

    public async Task<int> ContarAsync()
    {
        return await _db.Facturas.CountAsync();
    }

    public async Task<int> ContarPorMesAsync(int mes, int año)
    {
        return await _db.Facturas
            .Where(f => f.Fecha.Month == mes && f.Fecha.Year == año)
            .CountAsync();
    }

    public async Task<decimal> SumarVentasPorMesAsync(int mes, int año)
    {
        return await _db.Facturas
            .Where(f => f.Fecha.Month == mes && f.Fecha.Year == año)
            .SumAsync(f => f.Total);
    }

    public async Task<int> ContarPorFechaAsync(DateTime fecha)
    {
        return await _db.Facturas.CountAsync(f => f.Fecha.Date == fecha.Date);
    }

    public async Task ActualizarAsync(Factura factura)
    {
        _db.Facturas.Update(factura);
        await _db.SaveChangesAsync();
    }
}
