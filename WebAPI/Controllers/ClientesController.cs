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
        // El filtro global en ApplicationDbContext excluye IsDeleted=true
        var clientes = await _context.Clientes.AsNoTracking().ToListAsync();
        return Ok(clientes);
    }

    // GET: api/clientes/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Cliente>> GetCliente(int id)
    {
        // Usar query que respete filtro global
        var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (cliente == null) return NotFound(new { message = "Cliente no encontrado" });
        return Ok(cliente);
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
    public async Task<IActionResult> PostCliente(Cliente cliente)
    {
        cliente.IsDeleted = false;
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        // Registrar evento
        _context.EventosActividad.Add(new EventoActividad
        {
            Titulo = "Nuevo cliente",
            Descripcion = $"Cliente: {cliente.NombreRazonSocial} agregado.",
            Icono = "fa-solid fa-user-plus",
            Color = "#007bff",
            Fecha = DateTime.Now
        });
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, cliente);
    }

    // PUT: api/clientes/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] Cliente cliente)
    {
        var clienteExistente = await _context.Clientes.FindAsync(id);
        if (clienteExistente == null) return NotFound();

        clienteExistente.TipoIdentificacion = cliente.TipoIdentificacion;
        clienteExistente.Identificacion = cliente.Identificacion;
        clienteExistente.NombreRazonSocial = cliente.NombreRazonSocial;
        clienteExistente.Telefono = cliente.Telefono;
        clienteExistente.Direccion = cliente.Direccion;
        clienteExistente.Email = cliente.Email;

        await _context.SaveChangesAsync();

        // Registrar evento
        _context.EventosActividad.Add(new EventoActividad
        {
            Titulo = "Cliente editado",
            Descripcion = $"Cliente: {cliente.NombreRazonSocial} editado.",
            Icono = "fa-solid fa-user-edit",
            Color = "#17a2b8",
            Fecha = DateTime.Now
        });
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/clientes/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return NotFound(new { message = "Cliente no encontrado" });

        // Soft delete
        cliente.IsDeleted = true;
        await _context.SaveChangesAsync();

        // Registrar evento
        _context.EventosActividad.Add(new EventoActividad
        {
            Titulo = "Cliente eliminado",
            Descripcion = $"Cliente: {cliente.NombreRazonSocial} eliminado (soft).",
            Icono = "fa-solid fa-user-minus",
            Color = "#dc3545",
            Fecha = DateTime.Now
        });
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
