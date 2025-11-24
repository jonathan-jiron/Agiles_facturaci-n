using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Application.Interfaces;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PdfController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPdfGenerator _pdfGenerator;

    public PdfController(ApplicationDbContext context, IPdfGenerator pdfGenerator)
    {
        _context = context;
        _pdfGenerator = pdfGenerator;
    }

    [HttpGet("factura/{id}")]
    public async Task<IActionResult> DescargarFacturaPdf(int id)
    {
        var factura = await _context.Facturas
            .Include(f => f.Cliente)
            .Include(f => f.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (factura == null)
            return NotFound("Factura no encontrada");

        if (string.IsNullOrEmpty(factura.XmlComprobante))
            return BadRequest("La factura no tiene XML generado");

        try
        {
            // Generar PDF desde el XML
            var pdfBytes = _pdfGenerator.Generar(factura.XmlComprobante);
            
            return File(pdfBytes, "application/pdf", $"Factura_{factura.Numero}.pdf");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error generando PDF: {ex.Message}");
        }
    }
}
