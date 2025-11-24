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
        var factura = await _service.CrearFacturaAsync(dto);
        return CreatedAtAction(nameof(Obtener), new { id = factura.Id }, factura);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Listar()
    {
        var list = await _service.ListarAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> Obtener(int id)
    {
        var f = await _service.ObtenerPorIdAsync(id);
        if (f == null) return NotFound();
        return Ok(f);
    }
}
