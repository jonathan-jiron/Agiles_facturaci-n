using Application.Interfaces;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class DummyProductLookup : IProductLookup
    {
        public Task<decimal> GetUnitPriceAsync(int productoId)
        {
            return Task.FromResult(0m);
        }
    }
}