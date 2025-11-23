using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IProductLookup
    {
        Task<decimal> GetUnitPriceAsync(int productoId);
    }
}
