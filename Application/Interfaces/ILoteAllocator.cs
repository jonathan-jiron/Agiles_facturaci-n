using Domain.Entities;
namespace Application.Interfaces;

/// <summary>
/// Asigna y decrementa lotes usando FIFO (fecha de ingreso más antigua primero).
/// </summary>
public interface ILoteAllocator
{
    Task<List<Lote>> ObtenerLotesDisponiblesAsync(int productoId);
    Task DescontarStockAsync(int loteId, int cantidad);
    Task<List<LoteAllocation>> AllocateAsync(int productoId, int cantidadRequerida);
}

public class LoteAllocation
{
    public int LoteId { get; set; }
    public int CantidadConsumida { get; set; }
}
