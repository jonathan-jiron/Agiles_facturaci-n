using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IFacturacionElectronicaService
    {
        /// <summary>
        /// Procesa todo el flujo de facturación electrónica:
        /// 1. Validación de XML
        /// 2. Generación de clave de acceso
        /// 3. Inserción en XML
        /// 4. Firma digital
        /// 5. Envío a SRI (Recepción y Autorización)
        /// 6. Generación de PDF
        /// 7. Envío por correo al cliente
        /// </summary>
        /// <param name="xmlOriginal">XML de la factura antes de firmar</param>
        /// <param name="emailCliente">Correo del receptor autorizado</param>
        /// <returns>Mensaje con estado final y clave de acceso</returns>
        Task<string> ProcesarFacturaAsync(string xmlOriginal, string emailCliente);
    }
}
