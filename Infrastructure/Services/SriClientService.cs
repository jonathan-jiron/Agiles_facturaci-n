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
            // MODO MOCK: Simula respuesta del SRI para pruebas locales
            var mockConfig = _config["SRI:UseMockSri"];
            Console.WriteLine($"[DEBUG] SRI:UseMockSri config value: '{mockConfig}'");
            
            var useMock = mockConfig?.ToLower() == "true";
            Console.WriteLine($"[DEBUG] useMock evaluated to: {useMock}");
            
            if (useMock)
            {
                Console.WriteLine("[MOCK SRI] Simulando recepción exitosa del SRI...");
                await Task.Delay(500); // Simula latencia de red
                return "Estado: RECIBIDA, Motivo: Comprobante recibido correctamente (MODO SIMULACIÓN)";
            }

            Console.WriteLine("[DEBUG] Modo mock NO activado, conectando al SRI real...");

            var endpoint = _config["SRI:RecepcionEndpoint"];
            var client = new RecepcionComprobantesOfflineClient(RecepcionComprobantesOfflineClient.EndpointConfiguration.RecepcionComprobantesOfflinePort, endpoint);

            var xmlFirmado = System.Convert.FromBase64String(xmlFirmadoBase64);

            // El método retorna un objeto con la propiedad Body
            var respuesta = await client.validarComprobanteAsync(xmlFirmado);

            // Accede a la respuesta real
            var resultado = respuesta.RespuestaRecepcionComprobante;
            var estado = resultado.estado;
            
            Console.WriteLine($"[DEBUG SRI] Estado General: {estado}");
            
            var sb = new System.Text.StringBuilder();
            if (resultado.comprobantes != null)
            {
                Console.WriteLine($"[DEBUG SRI] Comprobantes recibidos: {resultado.comprobantes.Length}");
                foreach (var comp in resultado.comprobantes)
                {
                    Console.WriteLine($"[DEBUG SRI] Comprobante Clave: {comp.claveAcceso}");
                    if (comp.mensajes != null)
                    {
                        Console.WriteLine($"[DEBUG SRI] Mensajes en comprobante: {comp.mensajes.Length}");
                        foreach (var m in comp.mensajes)
                        {
                            var msg = $"[{m.tipo}] {m.mensaje1} - {m.informacionAdicional}";
                            Console.WriteLine($"[DEBUG SRI] Mensaje: {msg}");
                            sb.AppendLine(msg);
                        }
                    }
                    else
                    {
                        Console.WriteLine("[DEBUG SRI] El comprobante no tiene mensajes.");
                    }
                }
            }
            else
            {
                Console.WriteLine("[DEBUG SRI] No se recibieron comprobantes en la respuesta.");
            }
            
            if (sb.Length == 0)
            {
                sb.AppendLine("No se encontraron mensajes de error detallados en la respuesta del SRI.");
                if (resultado.comprobantes == null)
                {
                    sb.AppendLine("La lista de comprobantes es NULL.");
                }
                else if (resultado.comprobantes.Length == 0)
                {
                    sb.AppendLine("La lista de comprobantes está vacía.");
                }
                else
                {
                    sb.AppendLine($"Se recibieron {resultado.comprobantes.Length} comprobantes, pero sin mensajes.");
                }
            }
            
            var motivo = sb.ToString();

            // Devuelve un string con el estado y motivo
            return $"Estado: {estado}, Motivo: {motivo}";
        }

        public async Task<string> EnviarAutorizacionAsync(string claveAcceso)
        {
            // MODO MOCK: Simula respuesta del SRI para pruebas locales
            var useMock = _config["SRI:UseMockSri"]?.ToLower() == "true";
            if (useMock)
            {
                Console.WriteLine("[MOCK SRI] Simulando autorización exitosa del SRI...");
                await Task.Delay(500); // Simula latencia de red
                
                // Genera un XML de autorización simulado COMPLETO Y VÁLIDO
                var xmlAutorizado = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<autorizacion>
    <estado>AUTORIZADO</estado>
    <numeroAutorizacion>{claveAcceso}</numeroAutorizacion>
    <fechaAutorizacion>{DateTime.Now:dd/MM/yyyy HH:mm:ss}</fechaAutorizacion>
    <ambiente>PRUEBAS</ambiente>
    <comprobante><![CDATA[<?xml version=""1.0"" encoding=""UTF-8""?>
<factura id=""comprobante"" version=""1.1.0"">
    <infoTributaria>
        <ambiente>1</ambiente>
        <tipoEmision>1</tipoEmision>
        <razonSocial>PRUEBA SISTEMA</razonSocial>
        <ruc>1805350442</ruc>
        <claveAcceso>{claveAcceso}</claveAcceso>
        <codDoc>01</codDoc>
        <estab>001</estab>
        <ptoEmi>001</ptoEmi>
        <secuencial>000000001</secuencial>
        <dirMatriz>DIRECCIÓN MATRIZ</dirMatriz>
    </infoTributaria>
    <infoFactura>
        <fechaEmision>{DateTime.Now:dd/MM/yyyy}</fechaEmision>
        <dirEstablecimiento>DIRECCIÓN</dirEstablecimiento>
        <obligadoContabilidad>NO</obligadoContabilidad>
        <tipoIdentificacionComprador>04</tipoIdentificacionComprador>
        <razonSocialComprador>CLIENTE PRUEBA</razonSocialComprador>
        <identificacionComprador>1234567890</identificacionComprador>
        <totalSinImpuestos>10.00</totalSinImpuestos>
        <totalDescuento>0.00</totalDescuento>
        <totalConImpuestos>
            <totalImpuesto>
                <codigo>2</codigo>
                <codigoPorcentaje>0</codigoPorcentaje>
                <baseImponible>10.00</baseImponible>
                <valor>0.00</valor>
            </totalImpuesto>
        </totalConImpuestos>
        <propina>0.00</propina>
        <importeTotal>10.00</importeTotal>
        <moneda>DOLAR</moneda>
    </infoFactura>
    <detalles>
        <detalle>
            <codigoPrincipal>001</codigoPrincipal>
            <descripcion>Producto de prueba</descripcion>
            <cantidad>1.000000</cantidad>
            <precioUnitario>10.000000</precioUnitario>
            <descuento>0.00</descuento>
            <precioTotalSinImpuesto>10.00</precioTotalSinImpuesto>
            <impuestos>
                <impuesto>
                    <codigo>2</codigo>
                    <codigoPorcentaje>0</codigoPorcentaje>
                    <tarifa>0.00</tarifa>
                    <baseImponible>10.00</baseImponible>
                    <valor>0.00</valor>
                </impuesto>
            </impuestos>
        </detalle>
    </detalles>
    <pagos>
        <pago>
            <formaPago>20</formaPago>
            <total>10.00</total>
            <plazo>0</plazo>
            <unidadTiempo>dias</unidadTiempo>
        </pago>
    </pagos>
</factura>]]></comprobante>
</autorizacion>";
                
                return $"Estado: AUTORIZADO, AutorizacionXml: {xmlAutorizado}, Motivo: Autorizado (MODO SIMULACIÓN)";
            }

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
