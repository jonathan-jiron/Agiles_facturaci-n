using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces;

public interface IDetalleFacturaRepository
{
    Task<DetalleFactura> AgregarAsync(DetalleFactura detalle);
    Task<List<DetalleFactura>> ListarPorFacturaAsync(int facturaId);
}
