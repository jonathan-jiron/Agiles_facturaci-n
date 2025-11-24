using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IXmlSigner
    {
        /// <summary>
        /// Firma un XML y devuelve el XML firmado como byte[]
        /// </summary>
        byte[] FirmarXml(string xmlComprobante);
    }
}
