using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Xml.Linq;
using DinkToPdf;
using DinkToPdf.Contracts;
using System.Globalization;

namespace Infrastructure.Services;

public class XmlGenerator : IXmlGenerator
{
    private readonly IConfiguration _config;
    private readonly IConverter _pdfConverter;
    private const string RucConsumidorFinal = "9999999999999";
    private const string NamespaceSRI = "http://www.sri.gob.ec/DTE/v1.0.0"; // Ajustar si usa otra versión

    public XmlGenerator(IConfiguration config, IConverter pdfConverter)
    {
        _config = config;
        _pdfConverter = pdfConverter;
    }

    public string GenerarFacturaXml(Factura factura, Cliente cliente)
    {
        XNamespace ns = NamespaceSRI;

        // Formato para números con 2 decimales (F2) o 6 decimales (F6)
        var culture = CultureInfo.InvariantCulture;

        XDocument xml = new XDocument(
            new XDeclaration("1.0", "UTF-8", "no"),
            new XElement(ns + "factura",
                new XAttribute("id", "comprobante"),
                new XAttribute("version", "1.1.0"), // Permite 6 decimales

                // --- infoTributaria ---
                new XElement(ns + "infoTributaria",
                    new XElement(ns + "ambiente", _config["SRI:Ambiente"]), // 1: Pruebas
                    new XElement(ns + "tipoEmision", _config["SRI:TipoEmision"]), // 1: Normal
                    new XElement(ns + "razonSocial", "PRUEBA SISTEMA DE RENTAS INTERNAS"),
                    new XElement(ns + "ruc", "1805350442"),
                    new XElement(ns + "claveAcceso", factura.ClaveAcceso),
                    new XElement(ns + "codDoc", "01"),
                    new XElement(ns + "estab", "001"),
                    new XElement(ns + "ptoEmi", "001"),
                    new XElement(ns + "secuencial", factura.Numero!.Split('-')[2]),
                    new XElement(ns + "dirMatriz", "DIRECCIÓN MATRIZ")
                ),

                // --- infoFactura ---
                new XElement(ns + "infoFactura",
                    new XElement(ns + "fechaEmision", factura.Fecha.ToString("dd/MM/yyyy")),
                    new XElement(ns + "dirEstablecimiento", "DIRECCIÓN ESTABLECIMIENTO"),
                    new XElement(ns + "obligadoContabilidad", "NO"),
                    new XElement(ns + "tipoIdentificacionComprador", cliente.Identificacion == RucConsumidorFinal ? "07" : "04"),
                    new XElement(ns + "razonSocialComprador", cliente.NombreRazonSocial),
                    new XElement(ns + "identificacionComprador", cliente.Identificacion),
                    new XElement(ns + "totalSinImpuestos", factura.Subtotal.ToString("F2", culture)),
                    new XElement(ns + "totalDescuento", "0.00"), // Asumo 0.00

                    // TOTALES CON IMPUESTOS (ASUMO UN SOLO IMPUESTO: IVA 15%)
                    new XElement(ns + "totalConImpuestos",
                        new XElement(ns + "totalImpuesto",
                            new XElement(ns + "codigo", "2"), // IVA
                            new XElement(ns + "codigoPorcentaje", "4"), // 15% (Usar '0' para 0%)
                            new XElement(ns + "baseImponible", factura.Subtotal.ToString("F2", culture)),
                            new XElement(ns + "valor", factura.Iva.ToString("F2", culture))
                        )
                    ),
                    new XElement(ns + "propina", "0.00"),
                    new XElement(ns + "importeTotal", factura.Total.ToString("F2", culture)),
                    new XElement(ns + "moneda", "DOLAR")
                ),

                // --- 1. DETALLES (REQUERIDO) ---
                new XElement(ns + "detalles",
                    factura.Detalles.Select(d =>
                        new XElement(ns + "detalle",
                            new XElement(ns + "codigoPrincipal", d.Producto?.Codigo ?? "001"),
                            new XElement(ns + "descripcion", d.Producto?.Nombre ?? "Servicio"),
                            new XElement(ns + "cantidad", d.Cantidad.ToString("F6", culture)), // Ficha 2.32 permite 6 decimales
                            new XElement(ns + "precioUnitario", d.PrecioUnitario.ToString("F6", culture)),
                            new XElement(ns + "descuento", "0.00"),
                            new XElement(ns + "precioTotalSinImpuesto", d.PrecioTotal.ToString("F2", culture)),
                            // IMPUESTOS POR DETALLE (REQUERIDO)
                            new XElement(ns + "impuestos",
                                new XElement(ns + "impuesto",
                                    new XElement(ns + "codigo", "2"), // IVA
                                    new XElement(ns + "codigoPorcentaje", "4"), // 12%
                                    new XElement(ns + "tarifa", "12.00"),
                                    new XElement(ns + "baseImponible", d.PrecioTotal.ToString("F2", culture)),
                                    new XElement(ns + "valor", d.Iva.ToString("F2", culture))
                                )
                            )
                        )
                    )
                ),

                // --- 2. PAGOS (REQUERIDO) ---
                new XElement(ns + "pagos",
                    new XElement(ns + "pago",
                        new XElement(ns + "formaPago", "20"), // 20: Transferencia (Común en ambiente de pruebas)
                        new XElement(ns + "total", factura.Total.ToString("F2", culture)),
                        new XElement(ns + "plazo", "0"),
                        new XElement(ns + "unidadTiempo", "dias")
                    )
                )

            // Si necesita infoAdicional, agréguela aquí
            )
        );

        return xml.ToString();
    }

    // ... (El resto del método GenerarRidePdf queda igual)
    public byte[] GenerarRidePdf(Factura factura, Cliente cliente)
    {
        // ... (Implementación DinkToPdf)
        string htmlContent = $"<html><body><h1>RIDE: Factura {factura.Numero}</h1>" +
                             $"<p>Clave Acceso: <b>{factura.ClaveAcceso}</b></p>" +
                             $"<p>Estado SRI: {factura.EstadoSRI}</p></body></html>";

        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = { PaperSize = PaperKind.A4, Orientation = Orientation.Portrait },
            Objects = { new ObjectSettings() { HtmlContent = htmlContent } }
        };

        return _pdfConverter.Convert(doc);
    }
}