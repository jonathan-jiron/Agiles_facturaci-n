using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/facturacion")]
public class FacturacionController : ControllerBase
{
    private readonly IFacturacionElectronicaService _facturaService;

    public FacturacionController(IFacturacionElectronicaService facturaService)
    {
        _facturaService = facturaService;
    }

    [HttpPost("{idVenta}")]
    public async Task<IActionResult> Facturar(int idVenta)
    {
        // Obtener XML y correo cliente desde BD
        string xml = "..."; // Deserializar desde tu capa actual
        string email = "...";

        var result = await _facturaService.ProcesarFacturaAsync(xml, email);
        return Ok(result);
    }
}
