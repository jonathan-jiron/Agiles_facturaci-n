using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces;

public interface IFacturaRepository
{
    Task<Factura> AgregarAsync(Factura factura);
    Task<Factura?> ObtenerPorIdAsync(int id);
    Task<Factura?> ObtenerPorNumeroAsync(string numero);
    Task<List<Factura>> ListarPorClienteAsync(int clienteId);
    Task<List<Factura>> ListarAsync();
    Task<int> ContarAsync();
    Task<int> ContarPorMesAsync(int mes, int año);
    Task<decimal> SumarVentasPorMesAsync(int mes, int año);
    Task<int> ContarPorFechaAsync(DateTime fecha);
    Task ActualizarAsync(Factura factura);
}
