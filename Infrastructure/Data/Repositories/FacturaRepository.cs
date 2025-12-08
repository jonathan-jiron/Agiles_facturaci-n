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

    public async Task<int> ContarPorEstablecimientoPuntoEmisionAsync(string establecimiento, string puntoEmision)
    {
        return await _db.Facturas
            .CountAsync(f => f.Establecimiento == establecimiento && f.PuntoEmision == puntoEmision);
    }

    public async Task ActualizarAsync(Factura factura)
    {
        _db.Facturas.Update(factura);
        await _db.SaveChangesAsync();
    }

    public async Task<Factura?> GetByIdWithDetailsAsync(int id)
    {
        return await _db.Facturas
            .Include(f => f.Detalles)
                .ThenInclude(d => d.Producto)
            .Include(f => f.Cliente)
            .Include(f => f.Pagos.OrderBy(p => p.Orden))
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task UpdateAsync(Factura factura)
    {
        // Asegurar que la entidad esté siendo rastreada
        var entry = _db.Entry(factura);
        if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Detached)
        {
            // Si está desvinculada, buscarla primero
            var existing = await _db.Facturas.FindAsync(factura.Id);
            if (existing != null)
            {
                // Copiar valores
                _db.Entry(existing).CurrentValues.SetValues(factura);
                factura = existing;
            }
            else
            {
                _db.Facturas.Update(factura);
            }
        }
        else
        {
            // Si ya está rastreada, marcar como modificada
            entry.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        }
        
        await _db.SaveChangesAsync();
    }

    public async Task<List<Factura>> ListarPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        return await _db.Facturas
            .Where(f => f.Fecha.Date >= fechaInicio.Date && f.Fecha.Date <= fechaFin.Date)
            .Include(f => f.Detalles)
                .ThenInclude(d => d.Producto)
            .Include(f => f.Cliente)
            .OrderByDescending(f => f.Fecha)
            .ToListAsync();
    }

    public async Task<List<Factura>> ListarPorClienteYRangoFechasAsync(int clienteId, DateTime fechaInicio, DateTime fechaFin)
    {
        return await _db.Facturas
            .Where(f => f.ClienteId == clienteId && f.Fecha.Date >= fechaInicio.Date && f.Fecha.Date <= fechaFin.Date)
            .Include(f => f.Detalles)
                .ThenInclude(d => d.Producto)
            .Include(f => f.Cliente)
            .OrderByDescending(f => f.Fecha)
            .ToListAsync();
    }
}
