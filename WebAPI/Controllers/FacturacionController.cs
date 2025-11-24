using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;


[ApiController]
[Route("api/facturacion")]
public class FacturacionController : ControllerBase
{
    private readonly IFacturacionElectronicaService _facturaService;
    private readonly Infrastructure.Data.ApplicationDbContext _context;
    private readonly IXmlGenerator _xmlGenerator;
    private readonly IClaveAccesoService _claveAccesoService;

    public FacturacionController(
        IFacturacionElectronicaService facturaService,
        Infrastructure.Data.ApplicationDbContext context,
        IXmlGenerator xmlGenerator,
        IClaveAccesoService claveAccesoService)
    {
        _facturaService = facturaService;
        _context = context;
        _xmlGenerator = xmlGenerator;
        _claveAccesoService = claveAccesoService;
    }

    [HttpPost("{idVenta}")]
    public async Task<IActionResult> Facturar(int idVenta)
    {
        var factura = await _context.Facturas
            .Include(f => f.Cliente)
            .Include(f => f.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(f => f.Id == idVenta);

        if (factura == null) return NotFound("Factura no encontrada");

        // Validar/Generar Clave de Acceso si falta
        if (string.IsNullOrEmpty(factura.ClaveAcceso))
        {
            // Asumimos tipo 1 (Factura), ambiente 1 (Pruebas)
            var parts = factura.Numero.Split('-');
            if (parts.Length == 3)
            {
                var serie = parts[0] + parts[1];
                var secuencial = parts[2];
                var codigoNumerico = "12345678"; // Puede ser aleatorio
                var tipoEmision = "1"; // Normal

                factura.ClaveAcceso = _claveAccesoService.Generar(
                    factura.Fecha, 
                    "01", 
                    "1805350442001", 
                    "1", 
                    serie, 
                    secuencial,
                    codigoNumerico,
                    tipoEmision
                );
            }
            else
            {
                 // Fallback if format is wrong (shouldn't happen with new FacturaService)
                 // Just log or throw?
                 return BadRequest("Formato de número de factura inválido.");
            }
        }

        // Generar XML
        string xml;
        try 
        {
            xml = _xmlGenerator.GenerarFacturaXml(factura, factura.Cliente);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generando XML: {ex.Message}");
        }

        factura.XmlComprobante = xml;
        await _context.SaveChangesAsync();

        // Enviar al SRI
        try
        {
            var result = await _facturaService.ProcesarFacturaAsync(xml, factura.Cliente.Correo);
            
            // Actualizar estado (ProcesarFacturaAsync podría no actualizar la entidad Factura directamente si no tiene acceso)
            // Asumimos que si no lanza excepción, fue procesado (o el string result tiene info).
            factura.EstadoSRI = "ENVIADO"; // O parsear result
            await _context.SaveChangesAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR SRI] {ex.ToString()}"); // Log completo a consola
            factura.EstadoSRI = "ERROR";
            factura.MotivoRechazo = ex.Message;
            await _context.SaveChangesAsync();
            return StatusCode(500, $"Error SRI: {ex.Message}");
        }
    }
}
