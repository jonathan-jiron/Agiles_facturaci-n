using Application.Interfaces;
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

    public async Task<List<LoteAllocation>> AllocateAsync(int productoId, int cantidadRequerida)
    {
        if (cantidadRequerida <= 0)
            throw new ArgumentException("La cantidad requerida debe ser mayor a 0.", nameof(cantidadRequerida));

        // Obtener lotes disponibles ordenados por FIFO (FechaIngreso ASC)
        var lotes = await _db.Lotes
            .Where(l => l.ProductoId == productoId && l.Cantidad > 0)
            .OrderBy(l => l.FechaIngreso)
            .ToListAsync();

        var stockDisponible = lotes.Sum(l => l.Cantidad);
        if (stockDisponible < cantidadRequerida)
        {
            throw new InvalidOperationException(
                $"Stock insuficiente. Disponible: {stockDisponible}, Requerido: {cantidadRequerida}");
        }

        var allocations = new List<LoteAllocation>();
        var pendiente = cantidadRequerida;

        foreach (var lote in lotes)
        {
            if (pendiente <= 0) break;

            var consumir = Math.Min(lote.Cantidad, pendiente);
            lote.Cantidad -= consumir;
            pendiente -= consumir;

            allocations.Add(new LoteAllocation
            {
                LoteId = lote.Id,
                CantidadConsumida = consumir
            });
        }

        // Guardar cambios en la BD (decrementar cantidades)
        await _db.SaveChangesAsync();

        return allocations;
    }
}
