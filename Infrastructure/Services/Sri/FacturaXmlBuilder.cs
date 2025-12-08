using System.Globalization;
using System.Xml.Linq;
using Domain.Entities;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Sri
{
    public class 
    FacturaXmlBuilder
    {
        private readonly SriOptions _opt;
        public FacturaXmlBuilder(IOptions<SriOptions> options) => _opt = options.Value;

        public string BuildFacturaXml(Factura f)
        {
            var subtotal12 = f.Detalles.Where(d => d.IvaLinea > 0)
                .Sum(d => d.Cantidad * d.PrecioUnitario - d.Descuento);

            var subtotal0 = f.Detalles.Where(d => d.IvaLinea == 0)
                .Sum(d => d.Cantidad * d.PrecioUnitario - d.Descuento);

            var descuentoTotal = f.Detalles.Sum(d => d.Descuento);

            var totalSinImpuestos = subtotal12 + subtotal0;
            var importeTotal = f.Total;
            var iva = f.Iva;

            var doc = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement("factura",
                    new XAttribute("id", "comprobante"),
                    new XAttribute("version", "1.1.0"),
                    InfoTributaria(f),
                    InfoFactura(f, subtotal12, subtotal0, descuentoTotal, totalSinImpuestos, importeTotal, iva),
                    Detalles(f),
                    InfoAdicional(f)
                )
            );
            return doc.ToString(SaveOptions.DisableFormatting);
        }

        private XElement InfoTributaria(Factura f) =>
            new XElement("infoTributaria",
                new XElement("ambiente", _opt.Ambiente),
                new XElement("tipoEmision", _opt.TipoEmision),
                new XElement("razonSocial", _opt.EmisorRazonSocial),
                new XElement("nombreComercial", _opt.EmisorNombreComercial),
                new XElement("ruc", _opt.EmisorRuc),
                new XElement("claveAcceso", f.ClaveAcceso ?? ""),
                new XElement("codDoc", "01"),
                new XElement("estab", f.Establecimiento.PadLeft(3, '0')),
                new XElement("ptoEmi", f.PuntoEmision.PadLeft(3, '0')),
                new XElement("secuencial", f.SecuencialSri ?? ExtraerSecuencialDelNumero(f.Numero)),
                new XElement("dirMatriz", _opt.DirMatriz)
            );

        private XElement InfoFactura(Factura f, decimal subtotal12, decimal subtotal0, decimal descuentoTotal, decimal totalSinImpuestos, decimal importeTotal, decimal iva)
        {
            string fecha = f.Fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

            return new XElement("infoFactura",
                new XElement("fechaEmision", fecha),
                new XElement("dirEstablecimiento", _opt.DirMatriz),
                new XElement("obligadoContabilidad", "NO"),
                new XElement("tipoIdentificacionComprador", MapTipoIdentificacion(f.Cliente?.TipoIdentificacion)),
                new XElement("razonSocialComprador", f.Cliente?.NombreRazonSocial ?? ""),
                new XElement("identificacionComprador", f.Cliente?.Identificacion ?? ""),
                new XElement("totalSinImpuestos", totalSinImpuestos.ToString("0.00", CultureInfo.InvariantCulture)),
                new XElement("totalDescuento", descuentoTotal.ToString("0.00", CultureInfo.InvariantCulture)),
                new XElement("totalConImpuestos",
                    new XElement("totalImpuesto",
                        new XElement("codigo", "2"), // IVA
                        new XElement("codigoPorcentaje", subtotal12 > 0 ? "4" : "0"),
                        new XElement("baseImponible", subtotal12.ToString("0.00", CultureInfo.InvariantCulture)),
                        new XElement("valor", iva.ToString("0.00", CultureInfo.InvariantCulture))
                    )
                ),
                new XElement("importeTotal", importeTotal.ToString("0.00", CultureInfo.InvariantCulture)),
                new XElement("moneda", "DOLAR"),
                new XElement("pagos",
                    new XElement("pago",
                        new XElement("formaPago", MapFormaPago(f.FormaPago)),
                        new XElement("total", importeTotal.ToString("0.00", CultureInfo.InvariantCulture))
                    )
                )
            );
        }

        private XElement Detalles(Factura f) =>
            new XElement("detalles",
                (f.Detalles ?? new List<DetalleFactura>()).Select(d =>
                    new XElement("detalle",
                        new XElement("codigoPrincipal", d.Producto?.Codigo ?? d.ProductoId.ToString()),
                        new XElement("descripcion", d.Producto?.Nombre ?? "ITEM"),
                        new XElement("cantidad", d.Cantidad.ToString("0.######", CultureInfo.InvariantCulture)),
                        new XElement("precioUnitario", d.PrecioUnitario.ToString("0.00", CultureInfo.InvariantCulture)),
                        new XElement("descuento", d.Descuento.ToString("0.00", CultureInfo.InvariantCulture)),
                        new XElement("precioTotalSinImpuesto", (d.Cantidad * d.PrecioUnitario - d.Descuento).ToString("0.00", CultureInfo.InvariantCulture)),
                        new XElement("impuestos",
                            new XElement("impuesto",
                                new XElement("codigo", "2"),
                                new XElement("codigoPorcentaje", d.IvaLinea > 0 ? "4" : "0"),
                                new XElement("tarifa", d.IvaLinea > 0 ? "15.00" : "0.00"),
                                new XElement("baseImponible", (d.Cantidad * d.PrecioUnitario - d.Descuento).ToString("0.00", CultureInfo.InvariantCulture)),
                                new XElement("valor", d.IvaLinea.ToString("0.00", CultureInfo.InvariantCulture))
                            )
                        )
                    )
                )
            );

        private XElement? InfoAdicional(Factura f)
        {
            var email = f.Cliente?.Email;
            var dir = f.Cliente?.Direccion;
            var campos = new List<XElement>();
            if (!string.IsNullOrWhiteSpace(email))
                campos.Add(new XElement("campoAdicional", new XAttribute("nombre", "Email"), email));
            if (!string.IsNullOrWhiteSpace(dir))
                campos.Add(new XElement("campoAdicional", new XAttribute("nombre", "Direccion"), dir));
            return campos.Count == 0 ? null : new XElement("infoAdicional", campos);
        }

        private static string MapTipoIdentificacion(string? tipo)
        {
            return (tipo ?? "").ToUpperInvariant() switch
            {
                "RUC" => "04",
                "CEDULA" => "05",
                "CÉDULA" => "05",
                "PASAPORTE" => "06",
                "CONSUMIDOR_FINAL" => "07",
                _ => "05"
            };
        }

        private static string MapFormaPago(string? formaPago)
        {
            // Códigos de forma de pago según catálogo SRI
            return (formaPago ?? "").ToUpperInvariant() switch
            {
                "EFECTIVO" => "01",
                "TRANSFERENCIA" => "15",
                "TARJETACREDITO" => "19",
                "TARJETADEBITO" => "20",
                "CHEQUE" => "16",
                _ => "01" // Por defecto efectivo
            };
        }

        private static string ExtraerSecuencialDelNumero(string numero)
        {
            // El número tiene formato: 001-001-000000001
            var partes = numero.Split('-');
            if (partes.Length == 3 && partes[2].Length == 9)
            {
                return partes[2];
            }
            // Si no tiene el formato esperado, intentar extraer los últimos 9 dígitos
            var soloDigitos = new string(numero.Where(char.IsDigit).ToArray());
            if (soloDigitos.Length >= 9)
            {
                return soloDigitos.Substring(soloDigitos.Length - 9);
            }
            return "000000001";
        }
    }
}