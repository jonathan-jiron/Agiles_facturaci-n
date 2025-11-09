using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public ClientesController(ApplicationDbContext context) => _context = context;

    // GET: api/clientes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
    {
        var clientes = await _context.Clientes.AsNoTracking().ToListAsync();
        return Ok(clientes);
    }

    // GET: api/clientes/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Cliente>> GetCliente(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return NotFound(new { message = "Cliente no encontrado" });
        return cliente;
    }

    // GET: api/clientes/search?search=texto
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Cliente>>> SearchClientes(string? search)
    {
        var q = _context.Clientes.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(c =>
                c.Identificacion.Contains(s) ||
                c.NombreRazonSocial.Contains(s));
        }
        return Ok(await q.AsNoTracking().ToListAsync());
    }

    private bool ValidarIdentificacion(string tipo, string identificacion)
    {
        if (tipo == "CEDULA" && identificacion.Length != 10)
            return false;
        if (tipo == "RUC" && identificacion.Length != 13)
            return false;
        // Pasaporte: puedes agregar reglas si lo necesitas
        return true;
    }

    // POST: api/clientes
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Cliente cliente)
    {
        if (!ValidarIdentificacion(cliente.TipoIdentificacion, cliente.Identificacion))
            return BadRequest("La identificación no tiene la longitud correcta para el tipo seleccionado.");

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, cliente);
    }

    // PUT: api/clientes/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] Cliente cliente)
    {
        if (!ValidarIdentificacion(cliente.TipoIdentificacion, cliente.Identificacion))
            return BadRequest("La identificación no tiene la longitud correcta para el tipo seleccionado.");

        var clienteExistente = await _context.Clientes.FindAsync(id);
        if (clienteExistente == null)
            return NotFound();

        clienteExistente.TipoIdentificacion = cliente.TipoIdentificacion;
        clienteExistente.Identificacion = cliente.Identificacion;
        clienteExistente.NombreRazonSocial = cliente.NombreRazonSocial;
        clienteExistente.Telefono = cliente.Telefono;
        clienteExistente.Direccion = cliente.Direccion;
        clienteExistente.Correo = cliente.Correo;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/clientes/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return NotFound(new { message = "Cliente no encontrado" });

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
