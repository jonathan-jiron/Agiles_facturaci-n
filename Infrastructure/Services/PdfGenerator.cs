using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using DinkToPdf;
using DinkToPdf.Contracts;
using System;
using System.Text;
using System.Xml;

namespace Infrastructure.Services
{
    public class PdfGenerator : IPdfGenerator
    {
        private readonly IConfiguration _config;
        private readonly IConverter _converter;

        public PdfGenerator(IConfiguration config, IConverter converter)
        {
            _config = config;
            _converter = converter;
        }

        public byte[] Generar(string xmlAutorizado)
        {
            var xml = new XmlDocument();
            xml.LoadXml(xmlAutorizado);

            // Obtener datos de la factura
            var razonSocial = xml.GetElementsByTagName("razonSocialComprador")[0]?.InnerText;
            var total = xml.GetElementsByTagName("importeTotal")[0]?.InnerText;
            var numeroAutorizacion = xml.GetElementsByTagName("numeroAutorizacion")[0]?.InnerText;
            var fechaAutorizacion = xml.GetElementsByTagName("fechaAutorizacion")[0]?.InnerText;
            var claveAcceso = xml.GetElementsByTagName("claveAcceso")[0]?.InnerText;

            // Generar código QR desde la clave de acceso (usando API pública del SRI)
            string urlQr = $"https://srienlinea.sri.gob.ec/comprobantes-electronicos?clave={claveAcceso}";

            // HTML base del PDF (puedes personalizarlo)
            string html = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial; }}
                        .titulo {{ font-size: 20px; font-weight: bold; }}
                        .cuadro {{ border: 1px solid #000; padding: 10px; }}
                    </style>
                </head>
                <body>
                    <div class='titulo'>FACTURA AUTORIZADA</div>
                    <p><b>Cliente:</b> {razonSocial}</p>
                    <p><b>Total:</b> {total}</p>
                    <p><b>Número de Autorización:</b> {numeroAutorizacion}</p>
                    <p><b>Fecha de Autorización:</b> {fechaAutorizacion}</p>
                    <p><b>Clave de Acceso:</b> {claveAcceso}</p>
                    <p><b>Validación:</b> {urlQr}</p>
                    <br/>
                    <div class='cuadro'>Versión preliminar del PDF RIDE</div>
                </body>
                </html>
            ";

            var documento = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
                    Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 }
                },
                Objects = {
                    new ObjectSettings
                    {
                        HtmlContent = html,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            };

            return _converter.Convert(documento);
        }
    }
}
