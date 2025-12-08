using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Sri
{
    public class SriSoapClient
    {
        private readonly HttpClient _http = new();
        private readonly SriOptions _opt;

        public SriSoapClient(IOptions<SriOptions> options) => _opt = options.Value;

        public async Task<(string Estado, string? MensajesXml)> EnviarRecepcionAsync(string xmlFirmado)
        {
            // SRI espera el xml firmado como byte[] (en SOAP suele ir base64)
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlFirmado));
            string soap = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ec=""http://ec.gob.sri.ws.recepcion"">
    <soapenv:Header/>
    <soapenv:Body>
        <ec:validarComprobante>
            <xml>{base64}</xml>
        </ec:validarComprobante>
    </soapenv:Body>
</soapenv:Envelope>";

            var req = new HttpRequestMessage(HttpMethod.Post, _opt.RecepcionWsdl.Replace("?wsdl", ""));
            req.Content = new StringContent(soap, Encoding.UTF8, "text/xml");
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
            var resp = await _http.SendAsync(req);
            var xmlResp = await resp.Content.ReadAsStringAsync();

            // Parse estado + mensajes
            var x = XDocument.Parse(xmlResp);
            var estado = x.Descendants().FirstOrDefault(e => e.Name.LocalName == "estado")?.Value ?? "SIN_ESTADO";
            var mensajes = x.Descendants().FirstOrDefault(e => e.Name.LocalName == "mensajes")?.ToString(SaveOptions.DisableFormatting);
            return (estado, mensajes);
        }

        public async Task<(string Estado, string? NumeroAutorizacion, string? FechaAutorizacion, string? ComprobanteCdata, string? Mensajes)>
            ConsultarAutorizacionAsync(string claveAcceso)
        {
            string soap = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ec=""http://ec.gob.sri.ws.autorizacion"">
    <soapenv:Header/>
    <soapenv:Body>
        <ec:autorizacionComprobante>
            <claveAccesoComprobante>{claveAcceso}</claveAccesoComprobante>
        </ec:autorizacionComprobante>
    </soapenv:Body>
</soapenv:Envelope>";

            var req = new HttpRequestMessage(HttpMethod.Post, _opt.AutorizacionWsdl.Replace("?wsdl", ""));
            req.Content = new StringContent(soap, Encoding.UTF8, "text/xml");
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
            var resp = await _http.SendAsync(req);
            var xmlResp = await resp.Content.ReadAsStringAsync();

            var x = XDocument.Parse(xmlResp);
            var autorizacion = x.Descendants().FirstOrDefault(e => e.Name.LocalName == "autorizacion");
            var estado = autorizacion?.Elements().FirstOrDefault(e => e.Name.LocalName == "estado")?.Value ?? "SIN_ESTADO";
            var fecha = autorizacion?.Elements().FirstOrDefault(e => e.Name.LocalName == "fechaAutorizacion")?.Value;
            var numero = autorizacion?.Elements().FirstOrDefault(e => e.Name.LocalName == "numeroAutorizacion")?.Value;
            var comp = autorizacion?.Elements().FirstOrDefault(e => e.Name.LocalName == "comprobante")?.Value;
            var mensajes = autorizacion?.Elements().FirstOrDefault(e => e.Name.LocalName == "mensajes")?.ToString(SaveOptions.DisableFormatting);

            return (estado, numero, fecha, comp, mensajes);
        }
    }
}