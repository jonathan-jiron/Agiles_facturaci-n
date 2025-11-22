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
}
