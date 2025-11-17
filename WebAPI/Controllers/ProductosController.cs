using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;     // ApplicationDbContext
using Domain.Entities;         // Producto, Lote

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public ProductosController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> Get()
        {
            var data = await _db.Productos
                .AsNoTracking()
                .Include(p => p.Lotes)
                .OrderBy(p => p.Id)
                .Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Lotes = p.Lotes.Select(l => new LoteDto
                    {
                        Id = l.Id,
                        NumeroLote = l.NumeroLote,
                        Cantidad = l.Cantidad,
                        PrecioUnitario = l.PrecioUnitario
                    }).ToList()
                })
                .ToListAsync();

            return Ok(data);
        }

        // Nuevo: obtener un producto por id
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductoDto>> GetById(int id)
        {
            var p = await _db.Productos
                .AsNoTracking()
                .Include(x => x.Lotes)
                .Where(x => x.Id == id)
                .Select(x => new ProductoDto
                {
                    Id = x.Id,
                    Codigo = x.Codigo,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,
                    Lotes = x.Lotes.Select(l => new LoteDto
                    {
                        Id = l.Id,
                        NumeroLote = l.NumeroLote,
                        Cantidad = l.Cantidad,
                        PrecioUnitario = l.PrecioUnitario
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (p is null) return NotFound();
            return Ok(p);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] ProductoCreateDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var exists = await _db.Productos.AnyAsync(x => x.Codigo == dto.Codigo);
            if (exists) return Conflict("El código ya existe.");

            var entity = new Producto
            {
                Codigo = dto.Codigo,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Lotes = dto.Lotes?.Select(l => new Lote
                {
                    NumeroLote = l.NumeroLote,
                    Cantidad = l.Cantidad,
                    PrecioUnitario = l.PrecioUnitario
                }).ToList() ?? new()
            };

            _db.Productos.Add(entity);
            await _db.SaveChangesAsync();

            // Registrar evento
            _db.EventosActividad.Add(new EventoActividad
            {
                Titulo = "Producto creado",
                Descripcion = $"Producto: {entity.Nombre} creado.",
                Icono = "fa-solid fa-box",
                Color = "#28a745",
                Fecha = DateTime.Now
            });
            await _db.SaveChangesAsync();

            // Apunta al nuevo endpoint GetById
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, null);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody] ProductoCreateDto dto)
        {
            var prod = await _db.Productos.Include(p => p.Lotes).FirstOrDefaultAsync(p => p.Id == id);
            if (prod is null) return NotFound();

            prod.Codigo = dto.Codigo;
            prod.Nombre = dto.Nombre;
            prod.Descripcion = dto.Descripcion;

            // Reemplazar lotes (simple y seguro)
            _db.Lotes.RemoveRange(prod.Lotes);
            prod.Lotes = dto.Lotes?.Select(l => new Lote
            {
                NumeroLote = l.NumeroLote,
                Cantidad = l.Cantidad,
                PrecioUnitario = l.PrecioUnitario
            }).ToList() ?? new();

            await _db.SaveChangesAsync();

            // Registrar evento
            _db.EventosActividad.Add(new EventoActividad
            {
                Titulo = "Producto editado",
                Descripcion = $"Producto: {prod.Nombre} editado.",
                Icono = "fa-solid fa-box-open",
                Color = "#17a2b8",
                Fecha = DateTime.Now
            });
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var prod = await _db.Productos.Include(p => p.Lotes).FirstOrDefaultAsync(p => p.Id == id);
            if (prod is null) return NotFound();

            _db.Lotes.RemoveRange(prod.Lotes);
            _db.Productos.Remove(prod);
            await _db.SaveChangesAsync();

            // Registrar evento
            _db.EventosActividad.Add(new EventoActividad
            {
                Titulo = "Producto eliminado",
                Descripcion = $"Producto: {prod.Nombre} eliminado.",
                Icono = "fa-solid fa-box",
                Color = "#dc3545",
                Fecha = DateTime.Now
            });
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }

    public class ProductoDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
        public List<LoteDto> Lotes { get; set; } = new();
    }
    public class LoteDto
    {
        public int Id { get; set; }
        public string NumeroLote { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
    public class ProductoCreateDto
    {
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
        public List<LoteCreateDto> Lotes { get; set; } = new();
    }
    public class LoteCreateDto
    {
        public string NumeroLote { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}

