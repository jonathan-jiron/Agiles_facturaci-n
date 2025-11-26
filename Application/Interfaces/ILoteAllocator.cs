namespace Application.Interfaces;

/// <summary>
/// Asigna y decrementa lotes usando FIFO (fecha de ingreso más antigua primero).
/// </summary>
public interface ILoteAllocator
{
    /// <summary>
    /// Asigna lotes por FIFO para un producto y cantidad. Decrementa el stock de los lotes.
    /// Retorna lista de (loteId, cantidadConsumida) para registrar en DetalleFactura.
    /// Lanza excepción si no hay suficiente stock.
    /// </summary>
    Task<List<LoteAllocation>> AllocateAsync(int productoId, int cantidadRequerida);
}

public class LoteAllocation
{
    public int LoteId { get; set; }
    public int CantidadConsumida { get; set; }
}
