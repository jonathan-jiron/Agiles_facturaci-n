using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ProductLookup : IProductLookup
{
    private readonly ApplicationDbContext _db;
    public ProductLookup(ApplicationDbContext db) => _db = db;

    public async Task<decimal> GetUnitPriceAsync(int productId)
    {
        var prod = await _db.Productos.FirstOrDefaultAsync(p => p.Id == productId);
        if (prod == null) throw new KeyNotFoundException($"Producto {productId} no encontrado");
        return prod.PrecioVenta;
    }
}
