using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IXmlGenerator
    {
        /// <summary>
        /// Genera el XML del comprobante (Factura) sin firmar, siguiendo el esquema del SRI.
        /// </summary>
        string GenerarFacturaXml(Factura factura, Cliente cliente);


    }
}
