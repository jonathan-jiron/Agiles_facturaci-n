using Application.DTOs;
using Domain.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;
using System.IO;

namespace Application.Services;

public class ReporteService
{
    private readonly IFacturaRepository _facturaRepo;

    public ReporteService(IFacturaRepository facturaRepo)
    {
        _facturaRepo = facturaRepo;
    }

    public async Task<ReporteVentasDto> GenerarReporteVentasAsync(DateTime fechaInicio, DateTime fechaFin, int? clienteId = null)
    {
        var facturas = clienteId.HasValue
            ? await _facturaRepo.ListarPorClienteYRangoFechasAsync(clienteId.Value, fechaInicio, fechaFin)
            : await _facturaRepo.ListarPorRangoFechasAsync(fechaInicio, fechaFin);

        var reporte = new ReporteVentasDto
        {
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            TotalFacturas = facturas.Count,
            TotalVentas = facturas.Sum(f => f.Total),
            TotalIva = facturas.Sum(f => f.Iva),
            TotalSubtotal = facturas.Sum(f => f.Subtotal)
        };

        // Ventas por día
        reporte.VentasPorDia = facturas
            .GroupBy(f => f.Fecha.Date)
            .Select(g => new ReporteVentasPorDiaDto
            {
                Fecha = g.Key,
                CantidadFacturas = g.Count(),
                TotalVentas = g.Sum(f => f.Total)
            })
            .OrderBy(v => v.Fecha)
            .ToList();

        // Ventas por cliente
        reporte.VentasPorCliente = facturas
            .GroupBy(f => new { f.ClienteId, ClienteNombre = f.Cliente?.NombreRazonSocial ?? "Sin nombre" })
            .Select(g => new ReporteVentasPorClienteDto
            {
                ClienteId = g.Key.ClienteId,
                ClienteNombre = g.Key.ClienteNombre,
                CantidadFacturas = g.Count(),
                TotalVentas = g.Sum(f => f.Total)
            })
            .OrderByDescending(v => v.TotalVentas)
            .ToList();

        // Productos vendidos
        var productosVendidos = facturas
            .SelectMany(f => f.Detalles.Select(d => new
            {
                ProductoId = d.ProductoId,
                ProductoCodigo = d.Producto?.Codigo ?? d.ProductoId.ToString(),
                ProductoNombre = d.Producto?.Nombre ?? "Producto",
                Cantidad = d.Cantidad,
                Total = d.PrecioUnitario * d.Cantidad - d.Descuento + d.IvaLinea
            }))
            .GroupBy(p => new { p.ProductoId, p.ProductoCodigo, p.ProductoNombre })
            .Select(g => new ReporteProductosVendidosDto
            {
                ProductoId = g.Key.ProductoId,
                ProductoCodigo = g.Key.ProductoCodigo,
                ProductoNombre = g.Key.ProductoNombre,
                CantidadVendida = g.Sum(p => p.Cantidad),
                TotalVentas = g.Sum(p => p.Total)
            })
            .OrderByDescending(p => p.TotalVentas)
            .ToList();

        reporte.ProductosVendidos = productosVendidos;

        return reporte;
    }

    public async Task<byte[]> GenerarPDFAsync(ReporteVentasDto reporte)
    {
        return await Task.Run(() =>
        {
            using var stream = new MemoryStream();
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(TextStyle.Default.FontSize(10));

                    page.Header().Column(header =>
                    {
                        header.Item().Row(row =>
                        {
                            row.RelativeItem().Text("REPORTE DE VENTAS").FontSize(18).Bold();
                            row.ConstantItem(200).AlignRight().Text($"Del {reporte.FechaInicio:dd/MM/yyyy} al {reporte.FechaFin:dd/MM/yyyy}").FontSize(10);
                        });
                        header.Item().PaddingBottom(10).BorderBottom(1).BorderColor(Colors.Grey.Medium);
                    });

                    page.Content().PaddingVertical(10).Column(content =>
                    {
                        // Resumen General
                        content.Item().PaddingBottom(10).Row(resumen =>
                        {
                            resumen.RelativeItem().Background(Colors.Grey.Lighten4).Padding(10).Column(col =>
                            {
                                col.Item().Text("RESUMEN GENERAL").FontSize(12).Bold();
                                col.Item().PaddingTop(5).Row(r =>
                                {
                                    r.RelativeItem().Text("Total Facturas:");
                                    r.ConstantItem(100).AlignRight().Text(reporte.TotalFacturas.ToString()).Bold();
                                });
                                col.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("Total Ventas:");
                                    r.ConstantItem(100).AlignRight().Text($"${reporte.TotalVentas:N2}").Bold();
                                });
                                col.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("Subtotal:");
                                    r.ConstantItem(100).AlignRight().Text($"${reporte.TotalSubtotal:N2}");
                                });
                                col.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("Total IVA:");
                                    r.ConstantItem(100).AlignRight().Text($"${reporte.TotalIva:N2}");
                                });
                            });
                        });

                        // Ventas por Día
                        if (reporte.VentasPorDia.Any())
                        {
                            content.Item().PaddingTop(10).Text("VENTAS POR DÍA").FontSize(12).Bold();
                            content.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(120);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCellStyle).Text("Fecha").Bold().FontSize(11).FontColor(Colors.Black);
                                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Facturas").Bold().FontSize(11).FontColor(Colors.Black);
                                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Total").Bold().FontSize(11).FontColor(Colors.Black);
                                });

                                foreach (var venta in reporte.VentasPorDia)
                                {
                                    table.Cell().Element(CellStyle).Text(venta.Fecha.ToString("dd/MM/yyyy"));
                                    table.Cell().Element(CellStyle).AlignRight().Text(venta.CantidadFacturas.ToString());
                                    table.Cell().Element(CellStyle).AlignRight().Text($"${venta.TotalVentas:N2}");
                                }
                            });
                        }

                        // Ventas por Cliente
                        if (reporte.VentasPorCliente.Any())
                        {
                            content.Item().PaddingTop(15).Text("VENTAS POR CLIENTE").FontSize(12).Bold();
                            content.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(120);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCellStyle).Text("Cliente").Bold().FontSize(11).FontColor(Colors.Black);
                                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Facturas").Bold().FontSize(11).FontColor(Colors.Black);
                                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Total").Bold().FontSize(11).FontColor(Colors.Black);
                                });

                                foreach (var venta in reporte.VentasPorCliente)
                                {
                                    table.Cell().Element(CellStyle).Text(venta.ClienteNombre);
                                    table.Cell().Element(CellStyle).AlignRight().Text(venta.CantidadFacturas.ToString());
                                    table.Cell().Element(CellStyle).AlignRight().Text($"${venta.TotalVentas:N2}");
                                }
                            });
                        }

                        // Productos Vendidos
                        if (reporte.ProductosVendidos.Any())
                        {
                            content.Item().PaddingTop(15).Text("PRODUCTOS MÁS VENDIDOS").FontSize(12).Bold();
                            content.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(80);
                                    columns.RelativeColumn(2);
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(120);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCellStyle).Text("Código").Bold().FontSize(11).FontColor(Colors.Black);
                                    header.Cell().Element(HeaderCellStyle).Text("Producto").Bold().FontSize(11).FontColor(Colors.Black);
                                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Cantidad").Bold().FontSize(11).FontColor(Colors.Black);
                                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Total").Bold().FontSize(11).FontColor(Colors.Black);
                                });

                                foreach (var producto in reporte.ProductosVendidos)
                                {
                                    table.Cell().Element(CellStyle).Text(producto.ProductoCodigo);
                                    table.Cell().Element(CellStyle).Text(producto.ProductoNombre);
                                    table.Cell().Element(CellStyle).AlignRight().Text(producto.CantidadVendida.ToString());
                                    table.Cell().Element(CellStyle).AlignRight().Text($"${producto.TotalVentas:N2}");
                                }
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text("Reporte generado el " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")).FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });

            document.GeneratePdf(stream);
            return stream.ToArray();
        });
    }

    public async Task<byte[]> GenerarExcelAsync(ReporteVentasDto reporte)
    {
        return await Task.Run(() =>
        {
            using var workbook = new XLWorkbook();
            
            // Hoja 1: Resumen
            var resumenSheet = workbook.Worksheets.Add("Resumen");
            resumenSheet.Cell(1, 1).Value = "REPORTE DE VENTAS";
            resumenSheet.Cell(1, 1).Style.Font.Bold = true;
            resumenSheet.Cell(1, 1).Style.Font.FontSize = 14;
            
            resumenSheet.Cell(2, 1).Value = $"Del {reporte.FechaInicio:dd/MM/yyyy} al {reporte.FechaFin:dd/MM/yyyy}";
            
            resumenSheet.Cell(4, 1).Value = "RESUMEN GENERAL";
            resumenSheet.Cell(4, 1).Style.Font.Bold = true;
            
            resumenSheet.Cell(5, 1).Value = "Total Facturas:";
            resumenSheet.Cell(5, 2).Value = reporte.TotalFacturas;
            resumenSheet.Cell(6, 1).Value = "Total Ventas:";
            resumenSheet.Cell(6, 2).Value = reporte.TotalVentas;
            resumenSheet.Cell(6, 2).Style.NumberFormat.Format = "$#,##0.00";
            resumenSheet.Cell(7, 1).Value = "Subtotal:";
            resumenSheet.Cell(7, 2).Value = reporte.TotalSubtotal;
            resumenSheet.Cell(7, 2).Style.NumberFormat.Format = "$#,##0.00";
            resumenSheet.Cell(8, 1).Value = "Total IVA:";
            resumenSheet.Cell(8, 2).Value = reporte.TotalIva;
            resumenSheet.Cell(8, 2).Style.NumberFormat.Format = "$#,##0.00";

            // Hoja 2: Ventas por Día
            if (reporte.VentasPorDia.Any())
            {
                var ventasDiaSheet = workbook.Worksheets.Add("Ventas por Día");
                ventasDiaSheet.Cell(1, 1).Value = "Fecha";
                ventasDiaSheet.Cell(1, 2).Value = "Cantidad Facturas";
                ventasDiaSheet.Cell(1, 3).Value = "Total Ventas";
                ventasDiaSheet.Row(1).Style.Font.Bold = true;
                ventasDiaSheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

                int row = 2;
                foreach (var venta in reporte.VentasPorDia)
                {
                    ventasDiaSheet.Cell(row, 1).Value = venta.Fecha;
                    ventasDiaSheet.Cell(row, 1).Style.DateFormat.Format = "dd/MM/yyyy";
                    ventasDiaSheet.Cell(row, 2).Value = venta.CantidadFacturas;
                    ventasDiaSheet.Cell(row, 3).Value = venta.TotalVentas;
                    ventasDiaSheet.Cell(row, 3).Style.NumberFormat.Format = "$#,##0.00";
                    row++;
                }
                ventasDiaSheet.Columns().AdjustToContents();
            }

            // Hoja 3: Ventas por Cliente
            if (reporte.VentasPorCliente.Any())
            {
                var ventasClienteSheet = workbook.Worksheets.Add("Ventas por Cliente");
                ventasClienteSheet.Cell(1, 1).Value = "Cliente";
                ventasClienteSheet.Cell(1, 2).Value = "Cantidad Facturas";
                ventasClienteSheet.Cell(1, 3).Value = "Total Ventas";
                ventasClienteSheet.Row(1).Style.Font.Bold = true;
                ventasClienteSheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

                int row = 2;
                foreach (var venta in reporte.VentasPorCliente)
                {
                    ventasClienteSheet.Cell(row, 1).Value = venta.ClienteNombre;
                    ventasClienteSheet.Cell(row, 2).Value = venta.CantidadFacturas;
                    ventasClienteSheet.Cell(row, 3).Value = venta.TotalVentas;
                    ventasClienteSheet.Cell(row, 3).Style.NumberFormat.Format = "$#,##0.00";
                    row++;
                }
                ventasClienteSheet.Columns().AdjustToContents();
            }

            // Hoja 4: Productos Vendidos
            if (reporte.ProductosVendidos.Any())
            {
                var productosSheet = workbook.Worksheets.Add("Productos Vendidos");
                productosSheet.Cell(1, 1).Value = "Código";
                productosSheet.Cell(1, 2).Value = "Producto";
                productosSheet.Cell(1, 3).Value = "Cantidad Vendida";
                productosSheet.Cell(1, 4).Value = "Total Ventas";
                productosSheet.Row(1).Style.Font.Bold = true;
                productosSheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;

                int row = 2;
                foreach (var producto in reporte.ProductosVendidos)
                {
                    productosSheet.Cell(row, 1).Value = producto.ProductoCodigo;
                    productosSheet.Cell(row, 2).Value = producto.ProductoNombre;
                    productosSheet.Cell(row, 3).Value = producto.CantidadVendida;
                    productosSheet.Cell(row, 4).Value = producto.TotalVentas;
                    productosSheet.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
                    row++;
                }
                productosSheet.Columns().AdjustToContents();
            }

            resumenSheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        });
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

    private static IContainer CellStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten1)
            .Padding(8)
            .Background(Colors.White);
    }
}

