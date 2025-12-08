using Application.DTOs;
using Application.Services;
using Infrastructure.Services.Sri;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

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
    // [Authorize] // Temporalmente sin auth para pruebas SRI - descomentar en producción
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
    // [Authorize] // Temporalmente sin auth para pruebas SRI - descomentar en producción
    public async Task<IActionResult> Listar()
    {
        var list = await _service.ListarFacturasDtoAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    // [Authorize] // Temporalmente sin auth para pruebas SRI - descomentar en producción
    public async Task<IActionResult> Consultar(int id)
    {
        var dto = await _service.ConsultarFacturaDtoAsync(id);
        if (dto == null) return NotFound();
        return Ok(dto);
    }

    [HttpGet("{id}/pdf")]
    // [Authorize] // Temporalmente sin auth para pruebas - descomentar en producción
    public async Task<IActionResult> DescargarPDF(int id)
    {
        var pdfBytes = await _service.GenerarPDFAsync(id);
        if (pdfBytes == null)
            return NotFound();

        return File(pdfBytes, "application/pdf", $"Factura_{id}.pdf");
    }

    [HttpGet("{id}/xml")]
    // [Authorize] // Temporalmente sin auth para pruebas SRI - descomentar en producción
    public async Task<IActionResult> DescargarXML(int id, [FromServices] Domain.Interfaces.IFacturaRepository facturaRepo)
    {
        var factura = await facturaRepo.GetByIdWithDetailsAsync(id);
        if (factura == null)
            return NotFound();

        // Priorizar XML autorizado, luego firmado, luego generado
        var xml = factura.XmlAutorizado ?? factura.XmlFirmado ?? factura.XmlGenerado;
        if (string.IsNullOrWhiteSpace(xml))
            return NotFound("No se encontró XML para esta factura.");

        var xmlBytes = System.Text.Encoding.UTF8.GetBytes(xml);
        return File(xmlBytes, "application/xml", $"Factura_{factura.Numero}.xml");
    }

    [HttpPost("{id}/enviar-email")]
    // [Authorize] // Temporalmente sin auth para pruebas - descomentar en producción
    public async Task<IActionResult> EnviarPorEmail(int id)
    {
        try
        {
            var enviado = await _service.EnviarFacturaPorEmailAsync(id);
            if (!enviado)
                return BadRequest(new { error = "No se pudo enviar el correo. Verifique que la factura tenga un cliente con email válido." });

            return Ok(new { exito = true, mensaje = "Correo enviado correctamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error al enviar el correo", trace = ex.Message });
        }
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
    public async Task<ActionResult<object>> GetCrecimientoVentas()
    {
        var resultado = await _service.GetCrecimientoVentasAsync();
        return Ok(new { crecimiento = resultado.crecimiento, hayDatosAnterior = resultado.hayDatosAnterior });
    }

    [HttpGet("crecimiento-facturas")]
    public async Task<ActionResult<object>> GetCrecimientoFacturas()
    {
        var resultado = await _service.GetCrecimientoFacturasAsync();
        return Ok(new { crecimiento = resultado.crecimiento, hayDatosAnterior = resultado.hayDatosAnterior });
    }

    [HttpGet("crecimiento-clientes")]
    public async Task<ActionResult<object>> GetCrecimientoClientes()
    {
        var resultado = await _service.GetCrecimientoClientesAsync();
        return Ok(new { crecimiento = resultado.crecimiento, hayDatosAnterior = resultado.hayDatosAnterior });
    }

    [HttpGet("crecimiento-productos")]
    public async Task<ActionResult<object>> GetCrecimientoProductos()
    {
        var resultado = await _service.GetCrecimientoProductosAsync();
        return Ok(new { crecimiento = resultado.crecimiento, hayDatosAnterior = resultado.hayDatosAnterior });
    }

    [HttpGet("secuencia")]
    public async Task<ActionResult<object>> ObtenerSecuencia([FromQuery] DateTime fecha, 
        [FromServices] Domain.Interfaces.IFacturaRepository facturaRepo,
        [FromQuery] string? establecimiento = "001", 
        [FromQuery] string? puntoEmision = "001")
    {
        // Retorna secuencia en formato SRI para mostrar en UI
        var cantidad = await facturaRepo.ContarPorEstablecimientoPuntoEmisionAsync(establecimiento ?? "001", puntoEmision ?? "001");
        var secuencial = cantidad + 1;
        var numeroCompleto = $"{establecimiento?.PadLeft(3, '0') ?? "001"}-{puntoEmision?.PadLeft(3, '0') ?? "001"}-{secuencial:D9}";
        return Ok(new { secuencial, numeroCompleto });
    }
    
    [HttpGet("test-certificado")]
    public ActionResult<object> TestCertificado([FromServices] Infrastructure.Services.Sri.XadesSigner signer)
    {
        try
        {
            var valido = signer.TestCertificate();
            return Ok(new { 
                certificadoValido = valido,
                mensaje = valido ? "Certificado válido y accesible" : "Error al cargar el certificado. Verifica la ruta y contraseña."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                certificadoValido = false,
                mensaje = ex.Message 
            });
        }
    }

    [HttpGet("verificar-certificado-ruc")]
    public IActionResult VerificarCertificadoYRuc([FromServices] Infrastructure.Services.Sri.XadesSigner signer)
    {
        var resultado = signer.VerificarCertificadoYRuc();
        return Ok(resultado);
    }

    [HttpGet("configuracion-sri")]
    public ActionResult<object> GetConfiguracionSri([FromServices] IConfiguration config)
    {
        var sriConfig = config.GetSection("Sri");
        return Ok(new
        {
            Establecimiento = sriConfig["Estab"] ?? "001",
            PuntoEmision = sriConfig["PtoEmi"] ?? "001",
            EmisorRazonSocial = sriConfig["EmisorRazonSocial"] ?? ""
        });
    }

    [HttpPost("{id}/firmar")]
    // [Authorize] // Temporalmente sin auth para pruebas - descomentar en producción
    public async Task<IActionResult> Firmar(int id, 
        [FromServices] Domain.Interfaces.IFacturaRepository facturaRepo,
        [FromServices] Microsoft.Extensions.Options.IOptions<Infrastructure.Services.Sri.SriOptions> opt,
        [FromServices] Infrastructure.Services.Sri.ClaveAccesoGenerator claveGen,
        [FromServices] Infrastructure.Services.Sri.FacturaXmlBuilder xmlBuilder,
        [FromServices] Infrastructure.Services.Sri.XadesSigner signer)
    {
        try
        {
            var factura = await facturaRepo.GetByIdWithDetailsAsync(id);
            if (factura == null) return NotFound();

            // Extraer secuencial del número si no existe
            if (string.IsNullOrWhiteSpace(factura.SecuencialSri))
            {
                var partes = factura.Numero.Split('-');
                if (partes.Length == 3 && partes[2].Length == 9)
                {
                    factura.SecuencialSri = partes[2];
                }
                else
                {
                    factura.SecuencialSri = Infrastructure.Services.Sri.ClaveAccesoGenerator.Secuencial9(factura.Id);
                }
            }

            // Generar clave de acceso y XML
            var codNum = Infrastructure.Services.Sri.ClaveAccesoGenerator.CodigoNumerico8(factura.Id);
            factura.ClaveAcceso = claveGen.Generar(
                factura.Fecha,
                "01", opt.Value.EmisorRuc, opt.Value.Ambiente,
                factura.Establecimiento, factura.PuntoEmision,
                factura.SecuencialSri, codNum, opt.Value.TipoEmision);

            factura.XmlGenerado = xmlBuilder.BuildFacturaXml(factura);
            
            // Firmar el XML
            try
            {
                factura.XmlFirmado = signer.SignXml(factura.XmlGenerado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    error = "Error al firmar el XML", 
                    trace = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }

            // Actualizar estado a Firmada
            factura.Estado = Domain.Enums.EstadoFactura.Firmada;
            // Marcar como firmada pero pendiente de emisión al SRI
            factura.EstadoSri = "FIRMADA"; // Estado temporal hasta que se emita al SRI
            
            // Guardar todos los cambios
            await facturaRepo.UpdateAsync(factura);
            
            // Retornar respuesta con datos (NO 204 NoContent)
            return Ok(new { 
                exito = true,
                id = factura.Id, 
                numero = factura.Numero, 
                estado = factura.Estado.ToString(),
                estadoSri = factura.EstadoSri,
                claveAcceso = factura.ClaveAcceso,
                xmlFirmado = !string.IsNullOrWhiteSpace(factura.XmlFirmado) ? "Generado" : "No generado"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno", trace = ex.Message });
        }
    }

    [HttpPost("{id}/emitir-sri")]
    // [Authorize] // Temporalmente sin auth para pruebas SRI - descomentar en producción
    public async Task<IActionResult> EmitirSri(int id, [FromServices] SriIntegrationService sri)
    {
        var f = await sri.EmitirAsync(id);
        return Ok(new {
            f.Id,
            f.ClaveAcceso,
            f.EstadoSri,
            f.NumeroAutorizacion,
            f.FechaAutorizacion,
            f.MensajesSri
        });
    }
}
