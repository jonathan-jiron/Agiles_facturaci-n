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

        /// <summary>
        /// Genera la Representación Impresa del Documento Electrónico (RIDE) en formato PDF.
        /// </summary>
        byte[] GenerarRidePdf(Factura factura, Cliente cliente);
    }
}
