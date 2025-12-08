using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Services.Sri;
using Application.Services;

namespace Infrastructure.Services.Sri
{
    public class SriIntegrationService
    {
        private readonly SriOptions _opt;
        private readonly IFacturaRepository _facturas;
        private readonly ClaveAccesoGenerator _claveGen;
        private readonly FacturaXmlBuilder _xmlBuilder;
        private readonly XadesSigner _signer;
        private readonly SriSoapClient _soap;
        private readonly FacturaService? _facturaService;

        public SriIntegrationService(
            IOptions<SriOptions> opt,
            IFacturaRepository facturas,
            ClaveAccesoGenerator claveGen,
            FacturaXmlBuilder xmlBuilder,
            XadesSigner signer,
            SriSoapClient soap,
            FacturaService? facturaService)
        {
            _opt = opt.Value;
            _facturas = facturas;
            _claveGen = claveGen;
            _xmlBuilder = xmlBuilder;
            _signer = signer;
            _soap = soap;
            _facturaService = facturaService;
        }

        public async Task<Factura> EmitirAsync(int facturaId)
        {
            var f = await _facturas.GetByIdWithDetailsAsync(facturaId)
                ?? throw new InvalidOperationException("Factura no existe");

            // Si no tiene secuencial SRI, extraerlo del número de factura
            // El número tiene formato: 001-001-000000001
            if (string.IsNullOrWhiteSpace(f.SecuencialSri))
            {
                // Extraer secuencial del número (después del último guión)
                var partes = f.Numero.Split('-');
                if (partes.Length == 3 && partes[2].Length == 9)
                {
                    f.SecuencialSri = partes[2];
                }
                else
                {
                    // Fallback: usar el Id
                    f.SecuencialSri = ClaveAccesoGenerator.Secuencial9(f.Id);
                }
            }

            var codNum = ClaveAccesoGenerator.CodigoNumerico8(f.Id);
            f.ClaveAcceso = _claveGen.Generar(
                f.Fecha, // Usa la propiedad correcta
                "01", _opt.EmisorRuc, _opt.Ambiente,
                f.Establecimiento, f.PuntoEmision, f.SecuencialSri, codNum, _opt.TipoEmision);

            f.XmlGenerado = _xmlBuilder.BuildFacturaXml(f);
            f.XmlFirmado = _signer.SignXml(f.XmlGenerado);

            (string estadoRecep, string? mensajesRecep) = await _soap.EnviarRecepcionAsync(f.XmlFirmado);
            f.EstadoSri = estadoRecep;
            f.MensajesSri = mensajesRecep;
            if (estadoRecep != "RECIBIDA")
            {
                f.Estado = EstadoFactura.Generada;
                await _facturas.UpdateAsync(f);
                return f;
            }

            await Task.Delay(TimeSpan.FromSeconds(_opt.EsperaAutorizacionSeg));
            for (int intento = 1; intento <= 5; intento++)
            {
                (string estadoAut, string? numero, string? fecha, string? comprobanteCdata, string? mensajesAut) =
                    await _soap.ConsultarAutorizacionAsync(f.ClaveAcceso);

                f.EstadoSri = estadoAut;
                f.MensajesSri = mensajesAut ?? f.MensajesSri;
                if (estadoAut == "AUTORIZADO")
                {
                    f.NumeroAutorizacion = numero ?? f.ClaveAcceso;
                    if (DateTimeOffset.TryParse(fecha, out var dto))
                        f.FechaAutorizacion = dto.DateTime;
                    f.XmlAutorizado = comprobanteCdata;
                    f.Estado = EstadoFactura.Autorizada;
                    await _facturas.UpdateAsync(f);
                    
                    // Enviar correo automáticamente cuando la factura es autorizada
                    if (_facturaService != null)
                    {
                        try
                        {
                            await _facturaService.EnviarFacturaPorEmailAsync(facturaId);
                        }
                        catch (Exception ex)
                        {
                            // Log del error pero no fallar el proceso de autorización
                            // El correo se puede enviar manualmente después si es necesario
                            System.Diagnostics.Debug.WriteLine($"Error al enviar correo automático: {ex.Message}");
                        }
                    }
                    
                    return f;
                }
                if (estadoAut == "NO AUTORIZADO" || estadoAut == "RECHAZADO")
                {
                    f.Estado = EstadoFactura.Generada;
                    await _facturas.UpdateAsync(f);
                    return f;
                }
                await Task.Delay(TimeSpan.FromSeconds(_opt.EsperaAutorizacionSeg));
            }
            await _facturas.UpdateAsync(f);
            return f;
        }
    }
}