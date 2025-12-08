using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly ReporteService _reporteService;

    public ReportesController(ReporteService reporteService)
    {
        _reporteService = reporteService;
    }

    [HttpPost("ventas")]
    // [Authorize] // Temporalmente sin auth para pruebas - descomentar en producción
    public async Task<IActionResult> GenerarReporteVentas([FromBody] ReporteRequestDto request)
    {
        try
        {
            var fechaInicio = request.FechaInicio ?? DateTime.Now.AddMonths(-1);
            var fechaFin = request.FechaFin ?? DateTime.Now;

            var reporte = await _reporteService.GenerarReporteVentasAsync(
                fechaInicio, 
                fechaFin, 
                request.ClienteId
            );

            return Ok(reporte);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error al generar reporte", trace = ex.Message });
        }
    }

    [HttpGet("ventas")]
    // [Authorize] // Temporalmente sin auth para pruebas - descomentar en producción
    public async Task<IActionResult> GenerarReporteVentasGet(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        [FromQuery] int? clienteId)
    {
        try
        {
            var inicio = fechaInicio ?? DateTime.Now.AddMonths(-1);
            var fin = fechaFin ?? DateTime.Now;

            var reporte = await _reporteService.GenerarReporteVentasAsync(inicio, fin, clienteId);

            return Ok(reporte);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error al generar reporte", trace = ex.Message });
        }
    }

    [HttpPost("ventas/exportar-pdf")]
    // [Authorize] // Temporalmente sin auth para pruebas - descomentar en producción
    public async Task<IActionResult> ExportarPDF([FromBody] ReporteRequestDto request)
    {
        try
        {
            var fechaInicio = request.FechaInicio ?? DateTime.Now.AddMonths(-1);
            var fechaFin = request.FechaFin ?? DateTime.Now;

            var reporte = await _reporteService.GenerarReporteVentasAsync(
                fechaInicio,
                fechaFin,
                request.ClienteId
            );

            var pdfBytes = await _reporteService.GenerarPDFAsync(reporte);
            var fileName = $"Reporte_Ventas_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error al generar PDF", trace = ex.Message });
        }
    }

    [HttpPost("ventas/exportar-excel")]
    // [Authorize] // Temporalmente sin auth para pruebas - descomentar en producción
    public async Task<IActionResult> ExportarExcel([FromBody] ReporteRequestDto request)
    {
        try
        {
            var fechaInicio = request.FechaInicio ?? DateTime.Now.AddMonths(-1);
            var fechaFin = request.FechaFin ?? DateTime.Now;

            var reporte = await _reporteService.GenerarReporteVentasAsync(
                fechaInicio,
                fechaFin,
                request.ClienteId
            );

            var excelBytes = await _reporteService.GenerarExcelAsync(reporte);
            var fileName = $"Reporte_Ventas_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}.xlsx";

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error al generar Excel", trace = ex.Message });
        }
    }
}

