using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Interfaces;

public interface IFacturaRepository
{
    Task<Factura> AgregarAsync(Factura factura);

    Task<Factura?> ObtenerPorIdAsync(int id);

    // MÉTODO NUEVO: Carga la Factura, Cliente, Detalles y Productos en una sola consulta.
    Task<Factura?> ObtenerFacturaParaSriAsync(int id);

    Task<Factura?> ObtenerPorNumeroAsync(string numero);
    Task<List<Factura>> ListarPorClienteAsync(int clienteId);
    Task<List<Factura>> ListarAsync();

    Task UpdateAsync(Factura factura);
    Task<Cliente?> GetClienteById(int clienteId);
}