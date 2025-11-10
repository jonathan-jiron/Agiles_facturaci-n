using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Data;

[ApiController]
[Route("api/actividad")]
public class ActividadController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ActividadController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetRecientes()
    {
        var actividades = _context.EventosActividad
            .OrderByDescending(e => e.Fecha)
            .Take(10)
            .ToList();

        return Ok(actividades);
    }
}