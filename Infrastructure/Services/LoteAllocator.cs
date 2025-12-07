using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class LoteAllocator : ILoteAllocator
{
    private readonly ApplicationDbContext _db;

    public LoteAllocator(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Lote>> ObtenerLotesDisponiblesAsync(int productoId)
    {
        return await _db.Lotes
            .Where(l => l.ProductoId == productoId && l.Cantidad > 0)
            .OrderBy(l => l.FechaIngreso)
            .ToListAsync();
    }

    public async Task DescontarStockAsync(int loteId, int cantidad)
    {
        var lote = await _db.Lotes.FindAsync(loteId);
        if (lote == null)
            throw new InvalidOperationException("Lote no encontrado");

        if (lote.Cantidad < cantidad)
            throw new InvalidOperationException("Stock insuficiente en el lote");

        lote.Cantidad -= cantidad;
        await _db.SaveChangesAsync();
    }

    public async Task<List<LoteAllocation>> AllocateAsync(int productoId, int cantidadRequerida)
    {
        var lotes = await ObtenerLotesDisponiblesAsync(productoId);
        var resultado = new List<LoteAllocation>();
        int restante = cantidadRequerida;

        foreach (var lote in lotes)
        {
            if (restante <= 0) break;
            int consumir = Math.Min(lote.Cantidad, restante);
            resultado.Add(new LoteAllocation { LoteId = lote.Id, CantidadConsumida = consumir });
            restante -= consumir;
        }

        if (restante > 0)
            throw new InvalidOperationException("Stock insuficiente para el producto");

        return resultado;
    }
}
