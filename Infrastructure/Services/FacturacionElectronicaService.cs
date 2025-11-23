using Application.Interfaces;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class FacturacionElectronicaService : IFacturacionElectronicaService
    {
        private readonly IXmlValidator _validator;
        private readonly IXmlSigner _signer;
        private readonly IClaveAccesoService _claveAcceso;
        private readonly ISriClientService _sriClient;
        private readonly IPdfGenerator _pdfGenerator;
        private readonly IEmailService _emailService;

        public FacturacionElectronicaService(
            IXmlValidator validator, IXmlSigner signer, IClaveAccesoService claveAcceso,
            ISriClientService sriClient, IPdfGenerator pdfGenerator, IEmailService emailService)
        {
            _validator = validator;
            _signer = signer;
            _claveAcceso = claveAcceso;
            _sriClient = sriClient;
            _pdfGenerator = pdfGenerator;
            _emailService = emailService;
        }

        public async Task<string> ProcesarFacturaAsync(string xmlOriginal, string emailCliente)
        {
            // Generar clave de acceso
            var claveAcceso = _claveAcceso.Generar(DateTime.Now, "01", "RUC_AQUI", "2", "001001", "000000123", "12345678", "1");

            // Insertar clave en XML
            xmlOriginal = xmlOriginal.Replace("<claveAcceso></claveAcceso>", $"<claveAcceso>{claveAcceso}</claveAcceso>");

            // Validar antes de firmar
            _validator.Validate(xmlOriginal, "Schemas/Factura_V2.xsd");

            // Firmar
            var xmlFirmado = _signer.FirmarXml(xmlOriginal);
            var xmlFirmadoStr = Encoding.UTF8.GetString(xmlFirmado);

            // Enviar recepción
            var resp = await _sriClient.EnviarRecepcionAsync(xmlFirmadoStr);
            if (!resp.Contains("RECIBIDA")) return $"Error SRI Recepción: {resp}";

            // Autorizar
            var autorizacion = await _sriClient.EnviarAutorizacionAsync(claveAcceso);
            if (!autorizacion.Contains("AUTORIZADO")) return $"Error SRI Autorización: {autorizacion}";

            // PDF
            var pdf = _pdfGenerator.Generar(autorizacion);

            // Email
            await _emailService.Enviar(emailCliente, xmlFirmado, pdf);

            return $"Factura autorizada – ClaveAcceso: {claveAcceso}";
        }
    }
}
