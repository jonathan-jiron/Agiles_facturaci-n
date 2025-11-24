using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using DinkToPdf;
using DinkToPdf.Contracts;
using System;
using System.Text;
using System.Xml;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class PdfGenerator : IPdfGenerator
    {
        private readonly IConfiguration _config;
        
        // Cola de trabajos para el hilo STA
        private static readonly BlockingCollection<PdfWorkItem> _queue = new BlockingCollection<PdfWorkItem>();
        private static readonly Thread _workerThread;
        private static BasicConverter _converter;

        // Estructura para los items de trabajo
        private class PdfWorkItem
        {
            public HtmlToPdfDocument Document { get; set; }
            public TaskCompletionSource<byte[]> Tcs { get; set; }
        }

        static PdfGenerator()
        {
            // Iniciar el hilo STA único y persistente
            _workerThread = new Thread(WorkerLoop)
            {
                IsBackground = true,
                Name = "PdfWorkerThread"
            };
            _workerThread.SetApartmentState(ApartmentState.STA);
            _workerThread.Start();
        }

        public PdfGenerator(IConfiguration config)
        {
            _config = config;
        }

        private static void WorkerLoop()
        {
            Console.WriteLine("[PDF WORKER] Iniciando hilo de trabajo PDF (STA)...");
            
            // Inicializar el convertidor UNA SOLA VEZ en este hilo
            try
            {
                _converter = new BasicConverter(new PdfTools());
                Console.WriteLine("[PDF WORKER] Convertidor inicializado correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PDF WORKER ERROR] Fallo al inicializar PdfTools: {ex.Message}");
                return;
            }

            // Procesar la cola
            foreach (var item in _queue.GetConsumingEnumerable())
            {
                try
                {
                    Console.WriteLine("[PDF WORKER] Procesando solicitud de PDF...");
                    byte[] pdfBytes = _converter.Convert(item.Document);
                    Console.WriteLine("[PDF WORKER] PDF generado exitosamente.");
                    item.Tcs.SetResult(pdfBytes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PDF WORKER ERROR] Error al generar PDF: {ex.Message}");
                    item.Tcs.SetException(ex);
                }
            }
        }

        public byte[] Generar(string xmlAutorizado)
        {
            // Envolver en Task.Run para no bloquear el hilo actual mientras esperamos al worker
            // Sin embargo, como el método es síncrono (devuelve byte[]), debemos esperar el resultado.
            // Usamos .GetAwaiter().GetResult() de manera segura porque el trabajo real ocurre en otro hilo.
            return GenerarAsync(xmlAutorizado).GetAwaiter().GetResult();
        }

        private async Task<byte[]> GenerarAsync(string xmlAutorizado)
        {
            try 
            {
                Console.WriteLine("[PDF DEBUG] Iniciando generación de PDF (Encolando)...");

                if (string.IsNullOrEmpty(xmlAutorizado))
                    throw new ArgumentNullException(nameof(xmlAutorizado));

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlAutorizado);
                Console.WriteLine("[PDF DEBUG] XML cargado correctamente.");

                // --- 1. EXTRAER EL XML INTERNO (FACTURA) ---
                XmlNode comprobanteNode = null;
                
                // Verificar si es un XML de autorización (con wrapper)
                var autorizacionNode = xmlDoc.GetElementsByTagName("autorizacion");
                if (autorizacionNode.Count > 0)
                {
                    Console.WriteLine("[PDF DEBUG] Detectado wrapper <autorizacion>");
                    var comprobanteCdata = autorizacionNode[0]["comprobante"]?.InnerText;
                    if (!string.IsNullOrEmpty(comprobanteCdata))
                    {
                        Console.WriteLine("[PDF DEBUG] Cargando XML interno del comprobante...");
                        var innerXml = new XmlDocument();
                        innerXml.LoadXml(comprobanteCdata);
                        comprobanteNode = innerXml.DocumentElement; // La raíz <factura>
                    }
                }
                
                // Si no hubo wrapper o falló, intentar usar el XML directo
                if (comprobanteNode == null)
                {
                    Console.WriteLine("[PDF DEBUG] Usando XML directo (sin wrapper o fallback)");
                    comprobanteNode = xmlDoc.DocumentElement;
                }

                // --- 2. PARSEAR DATOS ---
                Console.WriteLine("[PDF DEBUG] Extrayendo datos de cabecera...");
                var infoTributaria = comprobanteNode["infoTributaria"];
                var infoFactura = comprobanteNode["infoFactura"];

                string razonSocial = infoTributaria?["razonSocial"]?.InnerText ?? "N/A";
                string ruc = infoTributaria?["ruc"]?.InnerText ?? "N/A";
                string claveAcceso = infoTributaria?["claveAcceso"]?.InnerText ?? "N/A";
                string sec = infoTributaria?["secuencial"]?.InnerText ?? "000000000";
                string estab = infoTributaria?["estab"]?.InnerText ?? "001";
                string ptoEmi = infoTributaria?["ptoEmi"]?.InnerText ?? "001";
                string numeroFactura = $"{estab}-{ptoEmi}-{sec}";
                string dirMatriz = infoTributaria?["dirMatriz"]?.InnerText ?? "";

                string fechaEmision = infoFactura?["fechaEmision"]?.InnerText ?? DateTime.Now.ToString("dd/MM/yyyy");
                string clienteNombre = infoFactura?["razonSocialComprador"]?.InnerText ?? "Consumidor Final";
                string clienteId = infoFactura?["identificacionComprador"]?.InnerText ?? "9999999999999";
                string totalSinImpuestosRaw = infoFactura?["totalSinImpuestos"]?.InnerText ?? "0";
                string importeTotalRaw = infoFactura?["importeTotal"]?.InnerText ?? "0";

                decimal.TryParse(totalSinImpuestosRaw, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal totalSinImpuestosVal);
                decimal.TryParse(importeTotalRaw, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal importeTotalVal);

                string totalSinImpuestos = totalSinImpuestosVal.ToString("N2", System.Globalization.CultureInfo.InvariantCulture);
                string importeTotal = importeTotalVal.ToString("N2", System.Globalization.CultureInfo.InvariantCulture);

                // --- 3. DETALLES ---
                Console.WriteLine("[PDF DEBUG] Extrayendo detalles...");
                var sbDetalles = new StringBuilder();
                var detallesNode = comprobanteNode["detalles"];
                if (detallesNode != null)
                {
                    foreach (XmlNode detalle in detallesNode.ChildNodes)
                    {
                        string desc = detalle["descripcion"]?.InnerText ?? "Producto";
                        string cantStr = detalle["cantidad"]?.InnerText ?? "0";
                        string precioStr = detalle["precioUnitario"]?.InnerText ?? "0";
                        string totalStr = detalle["precioTotalSinImpuesto"]?.InnerText ?? "0";

                        decimal.TryParse(cantStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal cant);
                        decimal.TryParse(precioStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal precio);
                        decimal.TryParse(totalStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal total);

                        // Fallback calculation if total is 0
                        if (total == 0 && cant > 0 && precio > 0)
                        {
                            total = cant * precio;
                        }

                        sbDetalles.Append($@"
                            <tr>
                                <td style='padding: 5px; border-bottom: 1px solid #ddd;'>{cant.ToString("N2", System.Globalization.CultureInfo.InvariantCulture)}</td>
                                <td style='padding: 5px; border-bottom: 1px solid #ddd;'>{desc}</td>
                                <td style='padding: 5px; border-bottom: 1px solid #ddd; text-align: right;'>${precio.ToString("N2", System.Globalization.CultureInfo.InvariantCulture)}</td>
                                <td style='padding: 5px; border-bottom: 1px solid #ddd; text-align: right;'>${total.ToString("N2", System.Globalization.CultureInfo.InvariantCulture)}</td>
                            </tr>");
                    }
                }

                // --- 4. HTML TEMPLATE ---
                Console.WriteLine("[PDF DEBUG] Generando HTML...");
                string htmlContent = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; font-size: 12px;'>
                        <div style='text-align: center; margin-bottom: 20px;'>
                            <h2 style='margin: 0;'>{razonSocial}</h2>
                            <p style='margin: 5px 0;'>RUC: {ruc}</p>
                            <p style='margin: 5px 0;'>{dirMatriz}</p>
                        </div>

                        <div style='border: 1px solid #000; padding: 10px; margin-bottom: 20px;'>
                            <h3 style='margin-top: 0;'>FACTURA No. {numeroFactura}</h3>
                            <p><b>Fecha de Emisión:</b> {fechaEmision}</p>
                            <p><b>Clave de Acceso:</b> <br/>{claveAcceso}</p>
                        </div>

                        <div style='margin-bottom: 20px;'>
                            <p><b>Cliente:</b> {clienteNombre}</p>
                            <p><b>RUC/CI:</b> {clienteId}</p>
                        </div>

                        <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
                            <thead>
                                <tr style='background-color: #f2f2f2;'>
                                    <th style='text-align: left; padding: 8px; border-bottom: 2px solid #ddd;'>Cant.</th>
                                    <th style='text-align: left; padding: 8px; border-bottom: 2px solid #ddd;'>Descripción</th>
                                    <th style='text-align: right; padding: 8px; border-bottom: 2px solid #ddd;'>P. Unit</th>
                                    <th style='text-align: right; padding: 8px; border-bottom: 2px solid #ddd;'>Total</th>
                                </tr>
                            </thead>
                            <tbody>
                                {sbDetalles}
                            </tbody>
                        </table>

                        <div style='float: right; width: 40%;'>
                            <table style='width: 100%;'>
                                <tr>
                                    <td style='text-align: right;'><b>Subtotal:</b></td>
                                    <td style='text-align: right;'>${totalSinImpuestos}</td>
                                </tr>
                                <tr>
                                    <td style='text-align: right;'><b>Total:</b></td>
                                    <td style='text-align: right; font-size: 14px;'><b>${importeTotal}</b></td>
                                </tr>
                            </table>
                        </div>
                    </body>
                    </html>";

                // --- 5. ENCOLAR TRABAJO ---
                Console.WriteLine("[PDF DEBUG] Encolando trabajo para el hilo STA...");
                var tcs = new TaskCompletionSource<byte[]>();
                var doc = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                        ColorMode = ColorMode.Color,
                        Orientation = Orientation.Portrait,
                        PaperSize = PaperKind.A4,
                    },
                    Objects = {
                        new ObjectSettings() {
                            PagesCount = true,
                            HtmlContent = htmlContent,
                            WebSettings = { DefaultEncoding = "utf-8" }
                        }
                    }
                };

                _queue.Add(new PdfWorkItem { Document = doc, Tcs = tcs });

                // Esperar resultado
                return await tcs.Task;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PDF ERROR] {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw; // Re-throw para que el controlador lo maneje
            }
        }
    }
}
