using Domain.Entities;
using Application.DTOs;
using Domain.Interfaces;
using Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

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
        // Genera el número de factura
        var ultimo = await _facturaRepo.ContarAsync();
        string numero = $"{DateTime.Now:yyyyMMdd}-{ultimo + 1:D5}";

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

        var factura = new Factura
        {
            Numero = numero, // <-- Asigna el número generado
            ClienteId = dto.ClienteId,
            Fecha = dto.Fecha,
            Establecimiento = dto.Establecimiento,
            PuntoEmision = dto.PuntoEmision,
            FormaPago = dto.FormaPago,
            Observaciones = dto.Observaciones,
            Estado = EstadoFactura.Generada,
            Detalles = dto.Detalles.Select(d => new DetalleFactura
            {
                ProductoId = d.ProductoId,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Descuento = d.Descuento,
                IvaLinea = d.IvaLinea,
                LoteId = d.LoteId // Asignado arriba
            }).ToList()
        };

        // Calcula totales
        factura.Subtotal = factura.Detalles.Sum(d => d.PrecioUnitario * d.Cantidad);
        factura.Iva = factura.Detalles.Sum(d => d.IvaLinea);
        factura.Total = factura.Subtotal - factura.Detalles.Sum(d => d.Descuento) + factura.Iva;

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
        var factura = await _facturaRepo.ObtenerPorIdAsync(facturaId);
        if (factura == null) return null;

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.Header().Text($"Factura Electrónica #{factura.Numero}").FontSize(20).Bold();
                page.Content().Column(col =>
                {
                    col.Item().Text($"Fecha: {factura.Fecha:dd/MM/yyyy}");
                    col.Item().Text($"Cliente: {factura.Cliente?.NombreRazonSocial} ({factura.Cliente?.Identificacion})");
                    col.Item().Text($"Dirección: {factura.Cliente?.Direccion}");
                    col.Item().Text($"Email: {factura.Cliente?.Email}");
                    col.Item().Text($"Forma de Pago: {factura.FormaPago}");
                    col.Item().Text($"Observaciones: {factura.Observaciones}");

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.ConstantColumn(60);
                            columns.ConstantColumn(80);
                            columns.ConstantColumn(80);
                            columns.ConstantColumn(80);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Producto").Bold();
                            header.Cell().Text("Cant.").Bold();
                            header.Cell().Text("P.Unit").Bold();
                            header.Cell().Text("Desc.").Bold();
                            header.Cell().Text("Total").Bold();
                        });

                        foreach (var d in factura.Detalles)
                        {
                            table.Cell().Text(d.Producto?.Nombre ?? "Producto");
                            table.Cell().Text(d.Cantidad.ToString());
                            table.Cell().Text($"${d.PrecioUnitario:N2}");
                            table.Cell().Text($"${d.Descuento:N2}");
                            table.Cell().Text($"${(d.PrecioUnitario * d.Cantidad - d.Descuento + d.IvaLinea):N2}");
                        }
                    });

                    col.Item().Text($"Subtotal: ${factura.Subtotal:N2}");
                    col.Item().Text($"IVA: ${factura.Iva:N2}");
                    col.Item().Text($"Total: ${factura.Total:N2}").Bold();
                });
                page.Footer().AlignCenter().Text("Factura generada electrónicamente - SRI Ecuador");
            });
        });

        return pdf.GeneratePdf();
    }

    public async Task<bool> EnviarFacturaPorEmailAsync(int facturaId)
    {
        var factura = await _facturaRepo.ObtenerPorIdAsync(facturaId);
        if (factura == null || factura.Cliente?.Email == null) return false;

        var pdfBytes = await GenerarPDFAsync(facturaId);
        if (pdfBytes == null) return false;

        var message = new MimeMessage();
        var smtpServer = _config["EmailSettings:SmtpServer"];
        var smtpPortString = _config["EmailSettings:SmtpPort"];
        var smtpPort = !string.IsNullOrEmpty(smtpPortString) ? int.Parse(smtpPortString) : 587;
        var smtpUser = _config["EmailSettings:SmtpUser"];
        var smtpPass = _config["EmailSettings:SmtpPass"];
        var fromEmail = _config["EmailSettings:FromEmail"];
        var fromName = _config["EmailSettings:FromName"];

        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(factura.Cliente.NombreRazonSocial, factura.Cliente.Email));
        message.Subject = $"Factura Electrónica #{factura.Numero}";

        var builder = new BodyBuilder
        {
            TextBody = $"Estimado {factura.Cliente.NombreRazonSocial},\nAdjunto encontrará su factura electrónica #{factura.Numero}."
        };
        builder.Attachments.Add($"Factura_{factura.Numero}.pdf", pdfBytes, new ContentType("application", "pdf"));
        message.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(smtpServer, smtpPort, false);
        await smtp.AuthenticateAsync(smtpUser, smtpPass);
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);

        return true;
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

        factura.Subtotal = factura.Detalles.Sum(d => d.PrecioUnitario * d.Cantidad);
        factura.Iva = factura.Detalles.Sum(d => d.IvaLinea);
        factura.Total = factura.Subtotal - factura.Detalles.Sum(d => d.Descuento) + factura.Iva;

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

    public async Task<decimal> GetCrecimientoVentasAsync()
    {
        var mesActual = DateTime.Now.Month;
        var añoActual = DateTime.Now.Year;
        var mesAnterior = mesActual == 1 ? 12 : mesActual - 1;
        var añoAnterior = mesActual == 1 ? añoActual - 1 : añoActual;

        var ventasActual = await _facturaRepo.SumarVentasPorMesAsync(mesActual, añoActual);
        var ventasAnterior = await _facturaRepo.SumarVentasPorMesAsync(mesAnterior, añoAnterior);

        if (ventasAnterior == 0) return 0;
        return ((ventasActual - ventasAnterior) / ventasAnterior) * 100;
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
            Estado = f.Estado.ToString()
        }).ToList();
    }

    public async Task<FacturaConsultaDto?> ConsultarFacturaDtoAsync(int id)
    {
        var factura = await _facturaRepo.ObtenerPorIdAsync(id);
        if (factura == null) return null;

        // Calcula los totales según tus reglas de negocio
        decimal subtotal12 = factura.Detalles.Where(d => d.IvaLinea > 0).Sum(d => d.PrecioUnitario * d.Cantidad);
        decimal subtotal0 = factura.Detalles.Where(d => d.IvaLinea == 0).Sum(d => d.PrecioUnitario * d.Cantidad);
        decimal descuentoTotal = factura.Detalles.Sum(d => d.Descuento);
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
            Total = factura.Total,
            Subtotal12 = subtotal12,
            Subtotal0 = subtotal0,
            DescuentoTotal = descuentoTotal,
            Iva = iva,
            IceTotal = iceTotal,
            Detalles = factura.Detalles.Select(d => new DetalleFacturaConsultaDto
            {
                Cantidad = d.Cantidad,
                ProductoNombre = d.Producto?.Nombre ?? "",
                PrecioUnitario = d.PrecioUnitario,
                Descuento = d.Descuento,
                Subtotal = d.Cantidad * d.PrecioUnitario - d.Descuento + d.IvaLinea,
                DescuentoPorcentaje = 0 // Si tienes el cálculo, ponlo aquí
            }).ToList()
        };
    }
}

public interface ILoteAllocator
{
    Task<List<Lote>> ObtenerLotesDisponiblesAsync(int productoId);
    Task DescontarStockAsync(int loteId, int cantidad);
}
