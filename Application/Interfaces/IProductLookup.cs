namespace Application.Interfaces;

public interface IProductLookup
{
    /// <summary>
    /// Obtiene el precio unitario actual del producto.
    /// </summary>
    Task<decimal> GetUnitPriceAsync(int productId);
}
