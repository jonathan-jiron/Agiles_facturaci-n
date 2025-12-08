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
    Task<int> ContarPorEstablecimientoPuntoEmisionAsync(string establecimiento, string puntoEmision);
    Task ActualizarAsync(Factura factura);
    Task<Factura?> GetByIdWithDetailsAsync(int id);
    Task UpdateAsync(Factura factura);
    
    // Métodos para reportes
    Task<List<Factura>> ListarPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<List<Factura>> ListarPorClienteYRangoFechasAsync(int clienteId, DateTime fechaInicio, DateTime fechaFin);
}
