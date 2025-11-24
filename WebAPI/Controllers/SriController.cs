using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/sri")]
public class SriController : ControllerBase
{
    private readonly ISriClientService _sriClient;

    public SriController(ISriClientService sriClient)
    {
        _sriClient = sriClient;
    }

    [HttpPost("recepcion")]
    public async Task<IActionResult> EnviarRecepcion([FromBody] string xmlBase64)
    {
        var resultado = await _sriClient.EnviarRecepcionAsync(xmlBase64);
        return Ok(resultado);
    }

    [HttpGet("autorizacion/{claveAcceso}")]
    public async Task<IActionResult> ConsultarAutorizacion(string claveAcceso)
    {
        var resultado = await _sriClient.EnviarAutorizacionAsync(claveAcceso);
        return Ok(resultado);
    }
}