using Domain.Entities;
using Application.DTOs;
using Domain.Interfaces;
using Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.IO;
using ZXing;
using ZXing.Common;
using System.Drawing;
using System.Drawing.Imaging;

namespace Application.Services;

public class FacturaService
{
    private readonly IFacturaRepository _facturaRepo;
    private readonly IDetalleFacturaRepository _detalleRepo;
    private readonly Application.Interfaces.IProductLookup _productLookup;
    private readonly Application.Interfaces.ILoteAllocator _loteAllocator;
    private readonly IConfiguration _config;

    public FacturaService(
        IFacturaRepository facturaRepo, 
        IDetalleFacturaRepository detalleRepo, 
        Application.Interfaces.IProductLookup productLookup,
        Application.Interfaces.ILoteAllocator loteAllocator,
        IConfiguration config)
    {
        _facturaRepo = facturaRepo;
        _detalleRepo = detalleRepo;
        _productLookup = productLookup;
        _loteAllocator = loteAllocator;
        _config = config;
    }

    public async Task<FacturaDto> CrearFacturaAsync(FacturaCreateDto dto)
    {
        // Genera el número de factura en formato SRI: Establecimiento(3) + PuntoEmision(3) + Secuencial(9)
        // Ejemplo: 001001000000001
        var secuencial = await _facturaRepo.ContarPorEstablecimientoPuntoEmisionAsync(
            dto.Establecimiento, dto.PuntoEmision);
        secuencial++; // Siguiente secuencial
        
        // Formato SRI con guiones para legibilidad: Estab(3)-PtoEmi(3)-Secuencial(9)
        // Ejemplo: 001-001-000000001
        string numero = $"{dto.Establecimiento.PadLeft(3, '0')}-{dto.PuntoEmision.PadLeft(3, '0')}-{secuencial:D9}";

        // Validación de stock y asignación de lotes FIFO
        foreach (var detalle in dto.Detalles)
        {
            var lotes = await _loteAllocator.ObtenerLotesDisponiblesAsync(detalle.ProductoId);

            int cantidadRestante = detalle.Cantidad;
            foreach (var lote in lotes)
            {
                if (lote.Cantidad <= 0) continue;
                int cantidadAsignada = Math.Min(lote.Cantidad, cantidadRestante);

                // Descuenta el stock del lote
                await _loteAllocator.DescontarStockAsync(lote.Id, cantidadAsignada);

                // Asigna el lote al detalle
                detalle.LoteId = lote.Id;

                cantidadRestante -= cantidadAsignada;
                if (cantidadRestante <= 0) break;
            }

            if (cantidadRestante > 0)
                throw new InvalidOperationException($"Stock insuficiente para el producto ID {detalle.ProductoId}");
        }

        // Calcula totales primero
        // Subtotal sin descuento (para referencia)
        var subtotalSinDescuento = dto.Detalles.Sum(d => d.PrecioUnitario * d.Cantidad);
        var descuentoTotal = dto.Detalles.Sum(d => d.Descuento);
        // Subtotal después del descuento (ya calculado en cada detalle)
        var subtotalConDescuento = dto.Detalles.Sum(d => (d.PrecioUnitario * d.Cantidad) - d.Descuento);
        // IVA ya está calculado sobre el subtotal después del descuento en cada detalle
        var iva = dto.Detalles.Sum(d => d.IvaLinea);
        // Total = Subtotal (con descuento) + IVA
        var total = subtotalConDescuento + iva;

        // Validar que la suma de pagos no exceda el total
        var totalPagos = dto.Pagos?.Sum(p => p.Monto) ?? 0m;
        if (dto.Pagos != null && dto.Pagos.Any() && totalPagos > total)
        {
            throw new InvalidOperationException($"El total de pagos (${totalPagos:N2}) no puede exceder el total de la factura (${total:N2})");
        }

        // Si no hay pagos en la lista pero hay FormaPago (compatibilidad hacia atrás), crear un pago
        var pagos = new List<PagoFactura>();
        if (dto.Pagos != null && dto.Pagos.Any())
        {
            // Usar los pagos de la lista
            pagos = dto.Pagos.Select((p, index) => new PagoFactura
            {
                FormaPago = p.FormaPago,
                Monto = p.Monto,
                NumeroComprobante = p.NumeroComprobante,
                Orden = p.Orden > 0 ? p.Orden : index + 1
            }).ToList();
        }
        else if (!string.IsNullOrWhiteSpace(dto.FormaPago))
        {
            // Compatibilidad hacia atrás: crear un pago desde FormaPago
            pagos.Add(new PagoFactura
            {
                FormaPago = dto.FormaPago,
                Monto = total,
                NumeroComprobante = !string.IsNullOrWhiteSpace(dto.NumeroComprobante) ? dto.NumeroComprobante : null,
                Orden = 1
            });
        }

        // Determinar el valor de FormaPago para compatibilidad hacia atrás
        string formaPagoDescriptiva;
        if (pagos.Count == 1)
        {
            // Si hay un solo pago, usar esa forma de pago
            formaPagoDescriptiva = pagos[0].FormaPago;
        }
        else if (pagos.Count > 1)
        {
            // Si hay múltiples pagos, concatenar las formas de pago únicas
            var formasUnicas = pagos.Select(p => p.FormaPago).Distinct().ToList();
            if (formasUnicas.Count == 1)
            {
                // Si todos los pagos son de la misma forma, solo mostrar esa
                formaPagoDescriptiva = formasUnicas[0];
            }
            else
            {
                // Si hay diferentes formas de pago, concatenarlas
                formaPagoDescriptiva = string.Join(", ", formasUnicas);
            }
        }
        else
        {
            // Si no hay pagos en la lista, usar el valor del DTO (compatibilidad hacia atrás)
            formaPagoDescriptiva = dto.FormaPago;
        }

        var factura = new Factura
        {
            Numero = numero, // <-- Asigna el número generado
            ClienteId = dto.ClienteId,
            Fecha = dto.Fecha,
            Establecimiento = dto.Establecimiento,
            PuntoEmision = dto.PuntoEmision,
            FormaPago = formaPagoDescriptiva, // Valor descriptivo basado en los pagos
            Observaciones = dto.Observaciones,
            Estado = EstadoFactura.Generada,
            // Generar secuencial SRI de 9 dígitos para uso interno
            SecuencialSri = secuencial.ToString("D9"),
            Detalles = dto.Detalles.Select(d => new DetalleFactura
            {
                ProductoId = d.ProductoId,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Descuento = d.Descuento,
                IvaLinea = d.IvaLinea,
                LoteId = d.LoteId // Asignado arriba
            }).ToList(),
            Pagos = pagos
        };

        // Asignar totales calculados
        // Subtotal se guarda como el subtotal sin descuento (para referencia)
        factura.Subtotal = subtotalSinDescuento;
        factura.Iva = iva;
        factura.Total = total;

        await _facturaRepo.AgregarAsync(factura);

        // Mapea a DTO
        var dtoResult = new FacturaDto
        {
            Id = factura.Id,
            Numero = factura.Numero,
            Fecha = factura.Fecha,
            ClienteId = factura.ClienteId,
            Subtotal = factura.Subtotal,
            Iva = factura.Iva,
            Total = factura.Total,
            Estado = factura.Estado.ToString(), // <-- Convierte enum a string para el DTO
            Detalles = factura.Detalles.Select(d => new DetalleFacturaDto
            {
                ProductoId = d.ProductoId,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Descuento = d.Descuento,
                IvaLinea = d.IvaLinea
            }).ToList()
        };

        return dtoResult;
    }

    public Task<Factura?> ObtenerPorIdAsync(int id) => _facturaRepo.ObtenerPorIdAsync(id);

    public Task<List<Factura>> ListarAsync() => _facturaRepo.ListarAsync();

    public async Task<byte[]?> GenerarPDFAsync(int facturaId)
    {
        var factura = await _facturaRepo.GetByIdWithDetailsAsync(facturaId);
        if (factura == null) return null;

        // Obtener configuración SRI
        var emisorRuc = _config["Sri:EmisorRuc"] ?? "";
        var emisorRazonSocial = _config["Sri:EmisorRazonSocial"] ?? "";
        var emisorNombreComercial = _config["Sri:EmisorNombreComercial"] ?? "";
        var dirMatriz = _config["Sri:DirMatriz"] ?? "";
        var ambiente = _config["Sri:Ambiente"] == "1" ? "PRODUCCION" : "PRUEBAS";
        var tipoEmision = _config["Sri:TipoEmision"] == "1" ? "NORMAL" : "CONTINGENCIA";

        // Calcular subtotales por tarifa de IVA
        // Subtotal 15% (12% en el nombre pero es 15% IVA en Ecuador): Precio * Cantidad - Descuento
        var subtotal15 = factura.Detalles.Where(d => d.IvaLinea > 0 && d.PrecioUnitario > 0)
            .Sum(d => (d.PrecioUnitario * d.Cantidad) - d.Descuento);
        var subtotal5 = 0m;
        var subtotal0 = factura.Detalles.Where(d => d.IvaLinea == 0)
            .Sum(d => (d.PrecioUnitario * d.Cantidad) - d.Descuento);
        var descuentoTotal = factura.Detalles.Sum(d => d.Descuento);
        // IVA 15% ya está calculado en IvaLinea de cada detalle
        var iva15 = factura.Iva;
        var iva5 = 0m;

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(15);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(TextStyle.Default.FontSize(10));

                // Header con dos columnas: Emisor (izq) y Autorización (der)
                page.Header().Row(row =>
                {
                    // Columna izquierda: Logo y datos del emisor
                    row.RelativeItem().Column(col =>
                    {
                        // Logo
                        col.Item().PaddingBottom(5).Row(logoRow =>
                        {
                            // Intentar cargar el logo desde diferentes ubicaciones
                            var logoPath = GetLogoPath();
                            if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
                            {
                                logoRow.ConstantItem(80).Height(80)
                                    .Image(logoPath)
                                    .FitArea();
                            }
                            else
                            {
                                // Fallback: logo estilizado si no se encuentra la imagen
                                logoRow.ConstantItem(80).Height(80)
                                    .Background(Colors.Blue.Medium)
                                    .AlignCenter()
                                    .AlignMiddle()
                                    .Text("R\nSA")
                                    .FontSize(24)
                                    .Bold()
                                    .FontColor(Colors.White);
                            }
                            logoRow.RelativeItem().PaddingLeft(10).Column(logoCol =>
                            {
                                logoCol.Item().Text(emisorNombreComercial).FontSize(14).Bold();
                                logoCol.Item().Text("Facturación Electrónica").FontSize(10).FontColor(Colors.Grey.Medium);
                            });
                        });

                        // Datos del emisor
                        col.Item().PaddingTop(5).Column(emisorCol =>
                        {
                            emisorCol.Item().Text($"Emisor: {emisorRazonSocial}").Bold().FontSize(10);
                            emisorCol.Item().Text($"RUC: {emisorRuc}").FontSize(10);
                            emisorCol.Item().Text($"Matriz: {dirMatriz}").FontSize(10);
                            emisorCol.Item().Text($"Obligado a llevar contabilidad: SI").FontSize(10);
                        });
                    });

                    // Columna derecha: Información de autorización con fondo gris claro
                    row.ConstantItem(300).Background(Colors.Grey.Lighten4).Padding(12).Column(authCol =>
                    {
                        authCol.Item().Text("FACTURA").FontSize(20).Bold().AlignRight();
                        authCol.Item().Text($"No. {factura.Numero}").FontSize(13).Bold().AlignRight();
                        
                        if (!string.IsNullOrWhiteSpace(factura.NumeroAutorizacion))
                        {
                            authCol.Item().PaddingTop(5).Text($"Número de Autorización:").FontSize(9).AlignRight();
                            authCol.Item().Text(factura.NumeroAutorizacion).FontSize(8).AlignRight();
                        }
                        
                        if (factura.FechaAutorizacion.HasValue)
                        {
                            authCol.Item().Text($"Fecha y hora de Autorización:").FontSize(9).AlignRight();
                            authCol.Item().Text(factura.FechaAutorizacion.Value.ToString("dd/MM/yyyy HH:mm:ss")).FontSize(9).AlignRight();
                        }
                        
                        authCol.Item().Text($"Ambiente: {ambiente}").FontSize(9).AlignRight();
                        authCol.Item().Text($"Emisión: {tipoEmision}").FontSize(9).AlignRight();
                        
                        if (!string.IsNullOrWhiteSpace(factura.ClaveAcceso))
                        {
                            authCol.Item().PaddingTop(5).Text($"Clave de Acceso:").FontSize(9).AlignRight();
                            
                            // Generar código de barras
                            var barcodeBytes = GenerateBarcode(factura.ClaveAcceso);
                            if (barcodeBytes != null)
                            {
                                authCol.Item().PaddingTop(3).AlignRight().Image(barcodeBytes).FitArea();
                            }
                            
                            authCol.Item().PaddingTop(3).Text(factura.ClaveAcceso).FontSize(8).AlignRight();
                        }
                    });
                });

                page.Content().Column(contentCol =>
                {
                    // Información del comprador
                    contentCol.Item().PaddingTop(8).PaddingBottom(8).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Column(compradorCol =>
                    {
                        compradorCol.Item().Text("Razón Social:").FontSize(9).FontColor(Colors.Grey.Medium);
                        compradorCol.Item().Text(factura.Cliente?.NombreRazonSocial ?? "").FontSize(10).Bold();
                        compradorCol.Item().Row(compradorRow =>
                        {
                            compradorRow.RelativeItem().Column(compradorLeft =>
                            {
                                if (!string.IsNullOrWhiteSpace(factura.Cliente?.Direccion))
                                    compradorLeft.Item().Text($"Dirección: {factura.Cliente.Direccion}").FontSize(9);
                                compradorLeft.Item().Text($"Fecha Emisión: {factura.Fecha:dd/MM/yyyy}").FontSize(9);
                            });
                            compradorRow.RelativeItem().Column(compradorRight =>
                            {
                                compradorRight.Item().Text($"RUC/CI: {factura.Cliente?.Identificacion ?? ""}").FontSize(9);
                                if (!string.IsNullOrWhiteSpace(factura.Cliente?.Telefono))
                                    compradorRight.Item().Text($"Teléfono: {factura.Cliente.Telefono}").FontSize(9);
                                if (!string.IsNullOrWhiteSpace(factura.Cliente?.Email))
                                    compradorRight.Item().Text($"Correo: {factura.Cliente.Email}").FontSize(9);
                            });
                        });
                    });

                    // Tabla de productos
                    contentCol.Item().PaddingTop(6).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(90);  // Código Principal
                            columns.ConstantColumn(60);  // Cantidad
                            columns.RelativeColumn(2);     // Descripción (más ancha)
                            columns.RelativeColumn(1.5f);  // Detalles Adicionales
                            columns.ConstantColumn(75);  // Precio Unitario
                            columns.ConstantColumn(65);   // Descuento
                            columns.ConstantColumn(75);   // Total
                        });

                        // Header de la tabla sin fondo, solo texto en negrita (más grande)
                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCellStyle).Text("Código Principal").Bold().FontSize(11).FontColor(Colors.Black);
                            header.Cell().Element(HeaderCellStyle).Text("Cantidad").Bold().FontSize(11).FontColor(Colors.Black);
                            header.Cell().Element(HeaderCellStyle).Text("Descripción").Bold().FontSize(11).FontColor(Colors.Black);
                            header.Cell().Element(HeaderCellStyle).Text("Detalles Adicionales").Bold().FontSize(11).FontColor(Colors.Black);
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Precio Unitario").Bold().FontSize(11).FontColor(Colors.Black);
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Descuento").Bold().FontSize(11).FontColor(Colors.Black);
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("Total").Bold().FontSize(11).FontColor(Colors.Black);
                        });

                        // Filas de productos
                        foreach (var d in factura.Detalles)
                        {
                            // Usar el campo Codigo del producto, no el ID
                            var codigoPrincipal = !string.IsNullOrWhiteSpace(d.Producto?.Codigo) 
                                ? d.Producto.Codigo 
                                : (d.ProductoId > 0 ? d.ProductoId.ToString() : "");
                            var descripcion = d.Producto?.Nombre ?? "Producto";
                            // Detalles Adicionales siempre vacío en la tabla
                            var totalLinea = (d.PrecioUnitario * d.Cantidad - d.Descuento + d.IvaLinea);

                            table.Cell().Element(CellStyle).Text(codigoPrincipal).FontSize(9);
                            table.Cell().Element(CellStyle).AlignRight().Text(d.Cantidad.ToString("N2")).FontSize(9);
                            table.Cell().Element(CellStyle).Text(descripcion).FontSize(9);
                            table.Cell().Element(CellStyle).Text("").FontSize(9); // Detalles Adicionales vacío
                            table.Cell().Element(CellStyle).AlignRight().Text($"${d.PrecioUnitario:N2}").FontSize(9);
                            table.Cell().Element(CellStyle).AlignRight().Text($"${d.Descuento:N2}").FontSize(9);
                            table.Cell().Element(CellStyle).AlignRight().Text($"${totalLinea:N2}").FontSize(9);
                        }
                    });

                    // Información Adicional y Formas de pago (debajo de la tabla)
                    contentCol.Item().PaddingTop(8).Row(infoRow =>
                    {
                        infoRow.RelativeItem().Column(infoCol =>
                        {
                            // Información Adicional
                            if (!string.IsNullOrWhiteSpace(factura.Observaciones))
                            {
                                infoCol.Item().Text("Información Adicional").FontSize(10).Bold();
                                infoCol.Item().PaddingTop(2).Text($"Descripción: {factura.Observaciones}").FontSize(9);
                            }
                            
                            // Formas de pago (múltiples)
                            if (factura.Pagos != null && factura.Pagos.Any())
                            {
                                infoCol.Item().PaddingTop(6).Text("Formas de pago").FontSize(10).Bold();
                                foreach (var pago in factura.Pagos.OrderBy(p => p.Orden))
                                {
                                    var pagoText = $"{pago.FormaPago}: ${pago.Monto:N2}";
                                    if (!string.IsNullOrWhiteSpace(pago.NumeroComprobante))
                                    {
                                        pagoText += $" (Comp: {pago.NumeroComprobante})";
                                    }
                                    infoCol.Item().PaddingTop(2).Text(pagoText).FontSize(9);
                                }
                                // Términos de pago (si aplica)
                                infoCol.Item().Text("0 días").FontSize(9);
                            }
                            else if (!string.IsNullOrWhiteSpace(factura.FormaPago))
                            {
                                // Compatibilidad hacia atrás: mostrar FormaPago si no hay pagos
                                infoCol.Item().PaddingTop(6).Text("Formas de pago").FontSize(10).Bold();
                                infoCol.Item().PaddingTop(2).Text($"{factura.FormaPago}: ${factura.Total:N2}").FontSize(9);
                                // Términos de pago (si aplica)
                                infoCol.Item().Text("0 días").FontSize(9);
                            }
                        });
                        infoRow.RelativeItem(); // Espacio vacío para alinear con el resumen
                    });

                    // Resumen de totales con fondo gris claro
                    contentCol.Item().PaddingTop(8).Row(totalesRow =>
                    {
                        totalesRow.RelativeItem(); // Espacio vacío
                        totalesRow.ConstantItem(220).Background(Colors.Grey.Lighten4).Padding(12).Column(totalesCol =>
                        {
                            totalesCol.Item().Row(subtotalRow =>
                            {
                                subtotalRow.RelativeItem().Text("Subtotal Sin Impuestos:").FontSize(9);
                                subtotalRow.ConstantItem(90).AlignRight().Text($"${factura.Subtotal:N2}").FontSize(9);
                            });
                            totalesCol.Item().Row(subtotal15Row =>
                            {
                                subtotal15Row.RelativeItem().Text("Subtotal 15%:").FontSize(9);
                                subtotal15Row.ConstantItem(90).AlignRight().Text($"${subtotal15:N2}").FontSize(9);
                            });
                            totalesCol.Item().Row(subtotal5Row =>
                            {
                                subtotal5Row.RelativeItem().Text("Subtotal 5%:").FontSize(9);
                                subtotal5Row.ConstantItem(90).AlignRight().Text($"${subtotal5:N2}").FontSize(9);
                            });
                            totalesCol.Item().Row(subtotal0Row =>
                            {
                                subtotal0Row.RelativeItem().Text("Subtotal 0%:").FontSize(9);
                                subtotal0Row.ConstantItem(90).AlignRight().Text($"${subtotal0:N2}").FontSize(9);
                            });
                            totalesCol.Item().Row(subtotalNoObjRow =>
                            {
                                subtotalNoObjRow.RelativeItem().Text("Subtotal No Objeto IVA:").FontSize(9);
                                subtotalNoObjRow.ConstantItem(90).AlignRight().Text("$0.00").FontSize(9);
                            });
                            totalesCol.Item().Row(descuentoRow =>
                            {
                                descuentoRow.RelativeItem().Text("Descuentos:").FontSize(9);
                                descuentoRow.ConstantItem(90).AlignRight().Text($"${descuentoTotal:N2}").FontSize(9);
                            });
                            totalesCol.Item().Row(iceRow =>
                            {
                                iceRow.RelativeItem().Text("ICE:").FontSize(9);
                                iceRow.ConstantItem(90).AlignRight().Text("$0.00").FontSize(9);
                            });
                            totalesCol.Item().Row(iva15Row =>
                            {
                                iva15Row.RelativeItem().Text("IVA 15%:").FontSize(9);
                                iva15Row.ConstantItem(90).AlignRight().Text($"${iva15:N2}").FontSize(9);
                            });
                            totalesCol.Item().Row(iva5Row =>
                            {
                                iva5Row.RelativeItem().Text("IVA 5%:").FontSize(9);
                                iva5Row.ConstantItem(90).AlignRight().Text($"${iva5:N2}").FontSize(9);
                            });
                            totalesCol.Item().Row(servicioRow =>
                            {
                                servicioRow.RelativeItem().Text("Servicio %:").FontSize(9);
                                servicioRow.ConstantItem(90).AlignRight().Text("$0.00").FontSize(9);
                            });
                            totalesCol.Item().PaddingTop(6).BorderTop(1).BorderColor(Colors.Grey.Medium).Row(totalRow =>
                            {
                                totalRow.RelativeItem().Text("Valor Total:").FontSize(11).Bold();
                                totalRow.ConstantItem(90).AlignRight().Text($"${factura.Total:N2}").FontSize(11).Bold();
                            });
                        });
                    });
                });

                page.Footer().AlignCenter().Text("Factura generada electrónicamente - SRI Ecuador").FontSize(9).FontColor(Colors.Grey.Medium);
            });
        });

        return pdf.GeneratePdf();
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten1)
            .Padding(8)
            .Background(Colors.White);
    }

    private static IContainer HeaderCellStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten1)
            .PaddingVertical(10)
            .PaddingHorizontal(8)
            .Background(Colors.White)
            .AlignMiddle();
    }

    private string? GetLogoPath()
    {
        // Buscar el logo en diferentes ubicaciones posibles
        var baseDir = AppContext.BaseDirectory;
        var possiblePaths = new[]
        {
            Path.Combine(baseDir, "wwwroot", "img", "logo.png"),
            Path.Combine(baseDir, "..", "..", "..", "WebAPI", "wwwroot", "img", "logo.png"),
            Path.Combine(baseDir, "..", "..", "..", "..", "WebAPI", "wwwroot", "img", "logo.png"),
            Path.GetFullPath(Path.Combine(baseDir, "wwwroot", "img", "logo.png")),
            Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "WebAPI", "wwwroot", "img", "logo.png"))
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
                return path;
        }

        return null;
    }

    private byte[]? GenerateBarcode(string text)
    {
        try
        {
            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Height = 60,
                    Width = 300,
                    Margin = 2,
                    PureBarcode = false
                }
            };

            var pixelData = writer.Write(text);
            
            // Convertir a imagen PNG en memoria
            using (var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppRgb))
            {
                var bitmapData = bitmap.LockBits(
                    new Rectangle(0, 0, pixelData.Width, pixelData.Height),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppRgb);

                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }

                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    return ms.ToArray();
                }
            }
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> EnviarFacturaPorEmailAsync(int facturaId)
    {
        var factura = await _facturaRepo.GetByIdWithDetailsAsync(facturaId);
        if (factura == null || factura.Cliente?.Email == null) return false;

        var pdfBytes = await GenerarPDFAsync(facturaId);
        if (pdfBytes == null) return false;

        // Obtener XML (priorizar autorizado, luego firmado, luego generado)
        var xml = factura.XmlAutorizado ?? factura.XmlFirmado ?? factura.XmlGenerado;
        byte[]? xmlBytes = null;
        if (!string.IsNullOrWhiteSpace(xml))
        {
            xmlBytes = System.Text.Encoding.UTF8.GetBytes(xml);
        }

        var message = new MimeMessage();
        var smtpServer = _config["EmailSettings:SmtpServer"];
        var smtpPortString = _config["EmailSettings:SmtpPort"];
        var smtpPort = !string.IsNullOrEmpty(smtpPortString) ? int.Parse(smtpPortString) : 587;
        var smtpUser = _config["EmailSettings:SmtpUser"];
        var smtpPass = _config["EmailSettings:SmtpPass"];
        var fromEmail = _config["EmailSettings:FromEmail"];
        var fromName = _config["EmailSettings:FromName"];

        // Validar configuración de correo
        if (string.IsNullOrWhiteSpace(smtpServer))
            throw new InvalidOperationException("Configuración de correo incompleta: SmtpServer no está configurado en appsettings.json");
        if (string.IsNullOrWhiteSpace(smtpUser))
            throw new InvalidOperationException("Configuración de correo incompleta: SmtpUser no está configurado en appsettings.json");
        if (string.IsNullOrWhiteSpace(smtpPass))
            throw new InvalidOperationException("Configuración de correo incompleta: SmtpPass no está configurado en appsettings.json");
        if (string.IsNullOrWhiteSpace(fromEmail))
            throw new InvalidOperationException("Configuración de correo incompleta: FromEmail no está configurado en appsettings.json");

        message.From.Add(new MailboxAddress(fromName ?? "Facturación Electrónica", fromEmail));
        message.To.Add(new MailboxAddress(factura.Cliente.NombreRazonSocial, factura.Cliente.Email));
        message.Subject = $"Factura Electrónica #{factura.Numero}";

        // Obtener configuración para el HTML
        var emisorRazonSocial = _config["Sri:EmisorRazonSocial"] ?? "REDROBAN S.A.";
        var emisorRuc = _config["Sri:EmisorRuc"] ?? "";
        var dirMatriz = _config["Sri:DirMatriz"] ?? "";

        // Formatear fecha en español
        var meses = new[] { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", 
                           "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
        var fechaFormateada = $"{meses[factura.Fecha.Month - 1]} {factura.Fecha.Day:D2}, {factura.Fecha.Year}";

        // Crear HTML profesional
        var htmlBody = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }}
        .container {{
            background-color: #ffffff;
            border-radius: 8px;
            padding: 30px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header {{
            border-top: 4px solid #4A90E2;
            padding-top: 20px;
            margin-bottom: 20px;
        }}
        .logo {{
            text-align: right;
            margin-bottom: 20px;
            color: #4A90E2;
            font-weight: bold;
            font-size: 18px;
        }}
        .greeting {{
            font-size: 18px;
            font-weight: bold;
            color: #2c3e50;
            margin-bottom: 15px;
        }}
        .message {{
            font-size: 14px;
            color: #555;
            margin-bottom: 20px;
        }}
        .invoice-info {{
            background-color: #f8f9fa;
            padding: 15px;
            border-radius: 5px;
            margin: 20px 0;
        }}
        .invoice-info p {{
            margin: 5px 0;
            font-size: 14px;
        }}
        .invoice-number {{
            font-weight: bold;
            color: #2c3e50;
        }}
        .amount {{
            font-size: 32px;
            font-weight: bold;
            color: #27ae60;
            margin: 20px 0;
            text-align: center;
        }}
        .amount-label {{
            font-size: 14px;
            color: #555;
            text-align: center;
            margin-bottom: 10px;
        }}
        .button-container {{
            text-align: center;
            margin: 30px 0;
        }}
        .button {{
            display: inline-block;
            background-color: #4A90E2;
            color: #ffffff;
            padding: 12px 30px;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            font-size: 16px;
            margin: 10px 5px;
        }}
        .button:hover {{
            background-color: #357ABD;
        }}
        .link {{
            color: #4A90E2;
            text-decoration: none;
            font-size: 14px;
        }}
        .footer {{
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #e0e0e0;
            font-size: 12px;
            color: #777;
        }}
        .footer p {{
            margin: 5px 0;
        }}
        .footer-company {{
            font-weight: bold;
            color: #2c3e50;
            margin-top: 10px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>{emisorRazonSocial}</div>
            <div class='greeting'>{factura.Cliente?.NombreRazonSocial?.ToUpper() ?? ""}</div>
            <div class='message'>
                Has recibido una Factura de<br>
                <strong>{emisorRazonSocial}</strong>
            </div>
        </div>

        <div class='invoice-info'>
            <p><span class='invoice-number'>FAC {factura.Numero}</span></p>
            <p>{fechaFormateada}</p>
        </div>

        <div class='amount-label'>Por el valor de:</div>
        <div class='amount'>${factura.Total:N2}</div>

        <div class='button-container'>
            <p style='font-size: 14px; color: #555; margin-bottom: 20px;'>
                Los archivos de su factura electrónica están adjuntos a este correo.
            </p>
            <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                <p style='font-size: 13px; color: #2c3e50; margin: 5px 0; font-weight: bold;'>
                    📎 Archivos adjuntos disponibles:
                </p>
                <p style='font-size: 12px; color: #555; margin: 8px 0;'>
                    📄 <strong>Factura_{factura.Numero}.pdf</strong> - Documento PDF de la factura<br>
                    {(xmlBytes != null ? $"📄 <strong>Factura_{factura.Numero}.xml</strong> - Archivo XML electrónico<br>" : "")}
                </p>
                <p style='font-size: 11px; color: #777; margin-top: 10px; font-style: italic;'>
                    Puede descargar estos archivos desde la sección de adjuntos de su cliente de correo.
                </p>
            </div>
        </div>

        <div class='footer'>
            <div class='footer-company'>{emisorRazonSocial}</div>
            <p>RUC {emisorRuc}</p>
            <p>{dirMatriz}</p>
            {(string.IsNullOrWhiteSpace(factura.ClaveAcceso) ? "" : $"<p style='margin-top: 10px; font-size: 11px; color: #999;'>Clave de Acceso: {factura.ClaveAcceso}</p>")}
        </div>
    </div>
</body>
</html>";

        // Texto plano como alternativa
        var textBody = $@"Estimado {factura.Cliente.NombreRazonSocial},

Has recibido una Factura de {emisorRazonSocial}

FAC {factura.Numero}
{fechaFormateada}

Por el valor de: ${factura.Total:N2}

{(string.IsNullOrWhiteSpace(factura.ClaveAcceso) ? "" : $"Clave de Acceso: {factura.ClaveAcceso}\n")}

Este correo contiene los archivos PDF y XML de su factura electrónica.

{emisorRazonSocial}
RUC {emisorRuc}
{dirMatriz}";

        var builder = new BodyBuilder
        {
            TextBody = textBody,
            HtmlBody = htmlBody
        };
        
        // Adjuntar PDF
        builder.Attachments.Add($"Factura_{factura.Numero}.pdf", pdfBytes, new ContentType("application", "pdf"));
        
        // Adjuntar XML si está disponible
        if (xmlBytes != null)
        {
            builder.Attachments.Add($"Factura_{factura.Numero}.xml", xmlBytes, new ContentType("application", "xml"));
        }
        
        message.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        try
        {
            await smtp.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(smtpUser, smtpPass);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al enviar correo: {ex.Message}. Verifique la configuración de EmailSettings en appsettings.json", ex);
        }
    }

    public async Task<FacturaDto> GuardarBorradorAsync(FacturaCreateDto dto)
    {
        var factura = new Factura
        {
            ClienteId = dto.ClienteId,
            Fecha = dto.Fecha,
            Establecimiento = dto.Establecimiento,
            PuntoEmision = dto.PuntoEmision,
            FormaPago = dto.FormaPago,
            Observaciones = dto.Observaciones,
            Estado = EstadoFactura.Borrador,
            Detalles = dto.Detalles.Select(d => new DetalleFactura
            {
                ProductoId = d.ProductoId,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Descuento = d.Descuento,
                IvaLinea = d.IvaLinea
            }).ToList()
        };

        // Subtotal sin descuento (para referencia)
        factura.Subtotal = factura.Detalles.Sum(d => d.PrecioUnitario * d.Cantidad);
        factura.Iva = factura.Detalles.Sum(d => d.IvaLinea);
        // Total = Subtotal (con descuento) + IVA
        var subtotalConDescuento = factura.Detalles.Sum(d => (d.PrecioUnitario * d.Cantidad) - d.Descuento);
        factura.Total = subtotalConDescuento + factura.Iva;

        await _facturaRepo.AgregarAsync(factura);

        var dtoResult = new FacturaDto
        {
            Id = factura.Id,
            Numero = factura.Numero,
            Fecha = factura.Fecha,
            ClienteId = factura.ClienteId,
            Subtotal = factura.Subtotal,
            Iva = factura.Iva,
            Total = factura.Total,
            Estado = factura.Estado.ToString(),
            Detalles = factura.Detalles.Select(d => new DetalleFacturaDto
            {
                ProductoId = d.ProductoId,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Descuento = d.Descuento,
                IvaLinea = d.IvaLinea
            }).ToList()
        };

        return dtoResult;
    }

    public async Task<int> GetTotalAsync()
    {
        return await _facturaRepo.ContarAsync();
    }

    public async Task<int> GetFacturasMesActualAsync()
    {
        var mes = DateTime.Now.Month;
        var año = DateTime.Now.Year;
        return await _facturaRepo.ContarPorMesAsync(mes, año);
    }

    public async Task<decimal> GetVentasMesActualAsync()
    {
        var mes = DateTime.Now.Month;
        var año = DateTime.Now.Year;
        return await _facturaRepo.SumarVentasPorMesAsync(mes, año);
    }


    public async Task<int> ContarClientesUnicosPorMesAsync(int mes, int año)
    {
        var facturas = await _facturaRepo.ListarAsync();
        return facturas
            .Where(f => f.Fecha.Month == mes && f.Fecha.Year == año)
            .Select(f => f.ClienteId)
            .Distinct()
            .Count();
    }

    public async Task<int> ContarProductosUnicosPorMesAsync(int mes, int año)
    {
        var facturas = await _facturaRepo.ListarAsync();
        return facturas
            .Where(f => f.Fecha.Month == mes && f.Fecha.Year == año)
            .SelectMany(f => f.Detalles)
            .Select(d => d.ProductoId)
            .Distinct()
            .Count();
    }

    public async Task<(decimal crecimiento, bool hayDatosAnterior)> GetCrecimientoClientesAsync()
    {
        var mesActual = DateTime.Now.Month;
        var añoActual = DateTime.Now.Year;
        var mesAnterior = mesActual == 1 ? 12 : mesActual - 1;
        var añoAnterior = mesActual == 1 ? añoActual - 1 : añoActual;

        var clientesActual = await ContarClientesUnicosPorMesAsync(mesActual, añoActual);
        var clientesAnterior = await ContarClientesUnicosPorMesAsync(mesAnterior, añoAnterior);

        if (clientesAnterior == 0) return (0, false);
        return (((clientesActual - clientesAnterior) / (decimal)clientesAnterior) * 100, true);
    }

    public async Task<(decimal crecimiento, bool hayDatosAnterior)> GetCrecimientoProductosAsync()
    {
        var mesActual = DateTime.Now.Month;
        var añoActual = DateTime.Now.Year;
        var mesAnterior = mesActual == 1 ? 12 : mesActual - 1;
        var añoAnterior = mesActual == 1 ? añoActual - 1 : añoActual;

        var productosActual = await ContarProductosUnicosPorMesAsync(mesActual, añoActual);
        var productosAnterior = await ContarProductosUnicosPorMesAsync(mesAnterior, añoAnterior);

        if (productosAnterior == 0) return (0, false);
        return (((productosActual - productosAnterior) / (decimal)productosAnterior) * 100, true);
    }

    public async Task<(decimal crecimiento, bool hayDatosAnterior)> GetCrecimientoFacturasAsync()
    {
        var mesActual = DateTime.Now.Month;
        var añoActual = DateTime.Now.Year;
        var mesAnterior = mesActual == 1 ? 12 : mesActual - 1;
        var añoAnterior = mesActual == 1 ? añoActual - 1 : añoActual;

        var facturasActual = await _facturaRepo.ContarPorMesAsync(mesActual, añoActual);
        var facturasAnterior = await _facturaRepo.ContarPorMesAsync(mesAnterior, añoAnterior);

        if (facturasAnterior == 0) return (0, false);
        return (((facturasActual - facturasAnterior) / (decimal)facturasAnterior) * 100, true);
    }

    public async Task<(decimal crecimiento, bool hayDatosAnterior)> GetCrecimientoVentasAsync()
    {
        var mesActual = DateTime.Now.Month;
        var añoActual = DateTime.Now.Year;
        var mesAnterior = mesActual == 1 ? 12 : mesActual - 1;
        var añoAnterior = mesActual == 1 ? añoActual - 1 : añoActual;

        var ventasActual = await _facturaRepo.SumarVentasPorMesAsync(mesActual, añoActual);
        var ventasAnterior = await _facturaRepo.SumarVentasPorMesAsync(mesAnterior, añoAnterior);

        if (ventasAnterior == 0) return (0, false);
        return (((ventasActual - ventasAnterior) / ventasAnterior) * 100, true);
    }

    public async Task<int> ContarFacturasAsync()
    {
        return await _facturaRepo.ContarAsync();
    }

    public async Task<int> ContarFacturasPorFechaAsync(DateTime fecha)
    {
        return await _facturaRepo.ContarPorFechaAsync(fecha);
    }

    public async Task ActualizarEstadoAsync(Factura factura)
    {
        factura.Estado = EstadoFactura.Firmada;
        await _facturaRepo.ActualizarAsync(factura);
    }

    public async Task<List<FacturaListDto>> ListarFacturasDtoAsync()
    {
        var facturas = await _facturaRepo.ListarAsync();
        return facturas.Select(f => new FacturaListDto
        {
            Id = f.Id,
            Numero = f.Numero,
            Fecha = f.Fecha,
            ClienteNombre = f.Cliente?.NombreRazonSocial ?? "",
            Total = f.Total,
            Estado = f.Estado.ToString(),
            EstadoSri = f.EstadoSri,
            ClaveAcceso = f.ClaveAcceso
        }).ToList();
    }

    public async Task<FacturaConsultaDto?> ConsultarFacturaDtoAsync(int id)
    {
        var factura = await _facturaRepo.GetByIdWithDetailsAsync(id);
        if (factura == null) return null;

        // Calcula los totales según tus reglas de negocio
        // Subtotal 12% (15% IVA): Precio * Cantidad - Descuento (solo productos con IVA)
        decimal subtotal12 = factura.Detalles.Where(d => d.IvaLinea > 0)
            .Sum(d => (d.PrecioUnitario * d.Cantidad) - d.Descuento);
        // Subtotal 0%: Precio * Cantidad - Descuento (solo productos sin IVA)
        decimal subtotal0 = factura.Detalles.Where(d => d.IvaLinea == 0)
            .Sum(d => (d.PrecioUnitario * d.Cantidad) - d.Descuento);
        decimal descuentoTotal = factura.Detalles.Sum(d => d.Descuento);
        // IVA es 15% del subtotal después del descuento (ya calculado en IvaLinea)
        decimal iva = factura.Detalles.Sum(d => d.IvaLinea);
        decimal iceTotal = 0; // Si tienes ICE, calcula aquí

        return new FacturaConsultaDto
        {
            Id = factura.Id,
            Numero = factura.Numero,
            Fecha = factura.Fecha,
            ClienteNombre = factura.Cliente?.NombreRazonSocial ?? "",
            Estado = factura.Estado.ToString(),
            Firmada = factura.Estado == EstadoFactura.Firmada,
            EstadoSri = factura.EstadoSri,
            ClaveAcceso = factura.ClaveAcceso,
            Total = factura.Total,
            Subtotal12 = subtotal12,
            Subtotal0 = subtotal0,
            DescuentoTotal = descuentoTotal,
            Iva = iva,
            IceTotal = iceTotal,
            Observaciones = factura.Observaciones ?? "",
            Detalles = factura.Detalles.Select(d => new DetalleFacturaConsultaDto
            {
                Cantidad = d.Cantidad,
                ProductoNombre = d.Producto?.Nombre ?? "",
                PrecioUnitario = d.PrecioUnitario,
                Descuento = d.Descuento,
                // Subtotal es Precio * Cantidad - Descuento (sin IVA)
                Subtotal = (d.PrecioUnitario * d.Cantidad) - d.Descuento,
                // Calcular el porcentaje de descuento: (Descuento / (Precio * Cantidad)) * 100
                DescuentoPorcentaje = d.PrecioUnitario * d.Cantidad > 0 
                    ? Math.Round((d.Descuento / (d.PrecioUnitario * d.Cantidad)) * 100m, 2) 
                    : 0
            }).ToList(),
            Pagos = factura.Pagos?.OrderBy(p => p.Orden).Select(p => new PagoFacturaConsultaDto
            {
                FormaPago = p.FormaPago,
                Monto = p.Monto,
                NumeroComprobante = p.NumeroComprobante,
                Orden = p.Orden
            }).ToList() ?? new List<PagoFacturaConsultaDto>()
        };
    }
}

public interface ILoteAllocator
{
    Task<List<Lote>> ObtenerLotesDisponiblesAsync(int productoId);
    Task DescontarStockAsync(int loteId, int cantidad);
}
