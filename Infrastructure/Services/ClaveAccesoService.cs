using Application.Interfaces;
using System;

namespace Infrastructure.Services
{
    public class ClaveAccesoService : IClaveAccesoService
    {
        public string Generar(DateTime fechaEmision, string tipoComprobante, string rucEmisor, string ambiente,
                                   string serie, string numeroComprobante, string codigoNumerico, string tipoEmision)
        {
            string fecha = fechaEmision.ToString("ddMMyyyy");
            string baseClave = $"{fecha}{tipoComprobante}{rucEmisor}{ambiente}{serie}{numeroComprobante}{codigoNumerico}{tipoEmision}";

            int suma = 0;
            int factor = 2;

            for (int i = baseClave.Length - 1; i >= 0; i--)
            {
                suma += int.Parse(baseClave[i].ToString()) * factor;
                factor = (factor == 7) ? 2 : factor + 1;
            }

            int modulo = suma % 11;
            int digitoVerificador = 11 - modulo;

            if (digitoVerificador == 10) digitoVerificador = 1;
            if (digitoVerificador == 11) digitoVerificador = 0;

            return baseClave + digitoVerificador;
        }
    }
}
