using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FacturasController : ControllerBase
{
    private readonly FacturaService _service;

    public FacturasController(FacturaService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Crear([FromBody] FacturaCreateDto dto)
    {
        try
        {
            var factura = await _service.CrearFacturaAsync(dto);
            return CreatedAtAction(nameof(Consultar), new { id = factura.Id }, factura);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Listar()
    {
        var list = await _service.ListarFacturasDtoAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> Consultar(int id)
    {
        var dto = await _service.ConsultarFacturaDtoAsync(id);
        if (dto == null) return NotFound();
        return Ok(dto);
    }

    [HttpGet("{id}/pdf")]
    [Authorize]
    public async Task<IActionResult> DescargarPDF(int id)
    {
        var pdfBytes = await _service.GenerarPDFAsync(id);
        if (pdfBytes == null)
            return NotFound();

        return File(pdfBytes, "application/pdf", $"Factura_{id}.pdf");
    }

    [HttpPost("{id}/enviar-email")]
    [Authorize]
    public async Task<IActionResult> EnviarPorEmail(int id)
    {
        var enviado = await _service.EnviarFacturaPorEmailAsync(id);
        if (!enviado)
            return BadRequest("No se pudo enviar el correo.");

        return Ok();
    }

    [HttpPost("borrador")]
    [Authorize]
    public async Task<IActionResult> GuardarBorrador([FromBody] FacturaCreateDto dto)
    {
        var factura = await _service.GuardarBorradorAsync(dto);
        return CreatedAtAction(nameof(Consultar), new { id = factura.Id }, factura);
    }

    [HttpGet("total")]
    public async Task<ActionResult<int>> GetTotalFacturas()
    {
        var total = await _service.GetTotalAsync();
        return Ok(total);
    }

    [HttpGet("mes-actual")]
    public async Task<ActionResult<int>> GetFacturasMesActual()
    {
        var totalMes = await _service.GetFacturasMesActualAsync();
        return Ok(totalMes);
    }

    [HttpGet("ventas-mes-actual")]
    public async Task<ActionResult<decimal>> GetVentasMesActual()
    {
        var ventasMes = await _service.GetVentasMesActualAsync();
        return Ok(ventasMes);
    }

    [HttpGet("crecimiento-ventas")]
    public async Task<ActionResult<decimal>> GetCrecimientoVentas()
    {
        var crecimiento = await _service.GetCrecimientoVentasAsync();
        return Ok(crecimiento);
    }

    [HttpGet("secuencia")]
    public async Task<ActionResult<string>> ObtenerSecuencia([FromQuery] DateTime fecha)
    {
        var cantidad = await _service.ContarFacturasPorFechaAsync(fecha.Date);
        var secuencia = $"{fecha:yyyyMMdd}-" + (cantidad + 1).ToString("D5");
        return Ok(secuencia);
    }

    [HttpPost("{id}/firmar")]
    [Authorize]
    public async Task<IActionResult> Firmar(int id)
    {
        var factura = await _service.ObtenerPorIdAsync(id);
        if (factura == null) return NotFound();

        factura.Estado = Domain.Enums.EstadoFactura.Firmada; // <-- Usa el enum
        await _service.ActualizarEstadoAsync(factura);

        return Ok();
    }
}
