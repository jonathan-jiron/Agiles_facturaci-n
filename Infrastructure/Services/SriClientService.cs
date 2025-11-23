using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class SriClientService : ISriClientService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public SriClientService(IConfiguration config)
        {
            _config = config;
            _httpClient = new HttpClient();
        }

        public async Task<string> EnviarRecepcionAsync(string xmlFirmado)
        {
            var url = _config["SRI:RecepcionUrl"];
            var requestXml = $"<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                $"<soap:Body>" +
                $"<ns:validarComprobante xmlns:ns=\"http://ec.gob.sri.ws.recepcion\"><xml>{xmlFirmado}</xml>" +
                $"</ns:validarComprobante>" +
                $"</soap:Body>" +
                $"</soap:Envelope>";
            var response = await _httpClient.PostAsync(url, new StringContent(requestXml, Encoding.UTF8, "text/xml"));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> EnviarAutorizacionAsync(string claveAcceso)
        {
            var url = _config["SRI:AutorizacionUrl"];
            var requestXml = $"<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body><ns:autorizacionComprobante xmlns:ns=\"http://ec.gob.sri.ws.autorizacion\"><claveAccesoComprobante>{claveAcceso}</claveAccesoComprobante></ns:autorizacionComprobante></soap:Body></soap:Envelope>";
            var response = await _httpClient.PostAsync(url, new StringContent(requestXml, Encoding.UTF8, "text/xml"));
            return await response.Content.ReadAsStringAsync();
        }
    }
}
