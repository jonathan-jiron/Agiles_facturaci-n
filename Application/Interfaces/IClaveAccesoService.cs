using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IClaveAccesoService
    {
        string Generar(DateTime fecha, string tipoComprobante, string ruc,
                       string ambiente, string serie, string secuencial,
                       string codigoNumerico, string tipoEmision);
    }

}
