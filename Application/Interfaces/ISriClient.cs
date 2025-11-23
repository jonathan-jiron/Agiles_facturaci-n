using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ISriClient
    {
        /// <summary>
        /// Envía el XML firmado al Web Service de Recepción del SRI.
        /// </summary>
        Task<(string estado, string? motivo)> EnviarComprobanteAsync(byte[] xmlFirmado);

        /// <summary>
        /// Consulta la autorización del comprobante al Web Service de Autorización del SRI.
        /// </summary>
        Task<(string estado, string? autorizacionXml, string? motivo)> ConsultarAutorizacionAsync(string claveAcceso);
    }
}
