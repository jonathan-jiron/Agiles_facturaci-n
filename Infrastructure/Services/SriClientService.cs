using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using RecepcionSRI;
using AutorizacionSRI;
using System.Threading.Tasks;
using System.Linq;

namespace Infrastructure.Services
{
    public class SriClientService : ISriClientService
    {
        private readonly IConfiguration _config;

        public SriClientService(IConfiguration config)
        {
            _config = config;
        }

        // La interfaz espera Task<string>
        public async Task<string> EnviarRecepcionAsync(string xmlFirmadoBase64)
        {
            var endpoint = _config["SRI:RecepcionEndpoint"];
            var client = new RecepcionComprobantesOfflineClient(RecepcionComprobantesOfflineClient.EndpointConfiguration.RecepcionComprobantesOfflinePort, endpoint);

            var xmlFirmado = System.Convert.FromBase64String(xmlFirmadoBase64);

            // El método retorna un objeto con la propiedad Body
            var respuesta = await client.validarComprobanteAsync(xmlFirmado);

            // Accede a la respuesta real
            var resultado = respuesta.RespuestaRecepcionComprobante;
            var estado = resultado.estado;
            var motivo = resultado.comprobantes?.FirstOrDefault()?.mensajes?.FirstOrDefault()?.mensaje1;

            // Devuelve un string con el estado y motivo
            return $"Estado: {estado}, Motivo: {motivo}";
        }

        public async Task<string> EnviarAutorizacionAsync(string claveAcceso)
        {
            var endpoint = _config["SRI:AutorizacionEndpoint"];
            var client = new AutorizacionComprobantesOfflineClient(AutorizacionComprobantesOfflineClient.EndpointConfiguration.AutorizacionComprobantesOfflinePort, endpoint);

            var respuesta = await client.autorizacionComprobanteAsync(claveAcceso);

            var resultado = respuesta.RespuestaAutorizacionComprobante;
            var autorizacion = resultado.autorizaciones?.FirstOrDefault();
            var estado = autorizacion?.estado;
            var autorizacionXml = autorizacion?.comprobante;
            var motivo = autorizacion?.mensajes?.FirstOrDefault()?.mensaje1;

            return $"Estado: {estado}, AutorizacionXml: {autorizacionXml}, Motivo: {motivo}";
        }

        public void MostrarConfiguracionSRI()
        {
            var ambiente = _config["SRI:Ambiente"];
            var recepcionEndpoint = _config["SRI:RecepcionEndpoint"];
            var autorizacionEndpoint = _config["SRI:AutorizacionEndpoint"];
            var usuario = _config["SRI:Usuario"];
            var clave = _config["SRI:Clave"];

            Console.WriteLine($"Ambiente: {ambiente}");
            Console.WriteLine($"Recepción: {recepcionEndpoint}");
            Console.WriteLine($"Autorización: {autorizacionEndpoint}");
            Console.WriteLine($"Usuario: {usuario}");
        }
    }
}
