using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ProductLookup : IProductLookup
    {
        private readonly ApplicationDbContext _db;

        public ProductLookup(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<decimal> GetUnitPriceAsync(int productoId)
        {
            var producto = await _db.Productos
                .FirstOrDefaultAsync(p => p.Id == productoId && !p.IsDeleted);
            
            return producto?.PrecioVenta ?? 0m;
        }
    }
}
