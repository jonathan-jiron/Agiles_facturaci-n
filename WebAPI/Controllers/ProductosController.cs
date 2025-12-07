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
                    PrecioVenta = p.PrecioVenta,
                    AplicaIva = p.AplicaIva,
                    Stock = p.Lotes.Sum(l => l.Cantidad), // <-- Cambia esto
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
                    PrecioVenta = x.PrecioVenta,
                    AplicaIva = x.AplicaIva,
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
                PrecioVenta = dto.PrecioVenta,
                AplicaIva = dto.AplicaIva,
                IsDeleted = false,
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
            prod.PrecioVenta = dto.PrecioVenta;
            prod.AplicaIva = dto.AplicaIva;

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

            // Soft delete
            prod.IsDeleted = true;
            await _db.SaveChangesAsync();

            // Registrar evento
            _db.EventosActividad.Add(new EventoActividad
            {
                Titulo = "Producto eliminado",
                Descripcion = $"Producto: {prod.Nombre} eliminado (soft).",
                Icono = "fa-solid fa-box",
                Color = "#dc3545",
                Fecha = DateTime.Now
            });
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // Nuevo endpoint: agregar lotes a un producto existente (importación automática)
        [HttpPost("{id:int}/lotes")]
        public async Task<ActionResult> AddLotes(int id, [FromBody] List<LoteCreateDto> lotes)
        {
            if (lotes == null || !lotes.Any())
                return BadRequest("No se proporcionaron lotes.");

            var prod = await _db.Productos.Include(p => p.Lotes).FirstOrDefaultAsync(p => p.Id == id);
            if (prod is null) return NotFound(new { message = "Producto no encontrado" });

            var added = new List<string>();
            var skipped = new List<string>();

            foreach (var l in lotes)
            {
                if (string.IsNullOrWhiteSpace(l.NumeroLote))
                {
                    skipped.Add("(sin número)");
                    continue;
                }

                // Evitar crear lote con mismo número (índice único global)
                var exists = await _db.Lotes.AnyAsync(x => x.NumeroLote == l.NumeroLote);
                if (exists)
                {
                    skipped.Add(l.NumeroLote);
                    continue;
                }

                var entity = new Lote
                {
                    NumeroLote = l.NumeroLote,
                    Cantidad = l.Cantidad,
                    PrecioUnitario = l.PrecioUnitario,
                    FechaIngreso = l.FechaIngreso ?? DateTime.Now,
                    FechaVencimiento = l.FechaVencimiento,
                    ProductoId = prod.Id
                };

                _db.Lotes.Add(entity);
                added.Add(l.NumeroLote);
            }

            await _db.SaveChangesAsync();

            // Registrar evento de importación si se añadió al menos uno
            if (added.Any())
            {
                _db.EventosActividad.Add(new EventoActividad
                {
                    Titulo = "Lotes importados",
                    Descripcion = $"Se importaron {added.Count} lote(s) al producto '{prod.Nombre}'.",
                    Icono = "fa-solid fa-boxes-stacked",
                    Color = "#ffc107",
                    Fecha = DateTime.Now
                });
                await _db.SaveChangesAsync();
            }

            return Ok(new { added, skipped });
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<List<ProductoDto>>> Buscar(string query)
        {
            var productos = await _db.Productos
                .Where(p => p.Nombre.Contains(query) || p.Codigo.Contains(query))
                .Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    PrecioVenta = p.PrecioVenta,
                    Stock = p.Lotes.Sum(l => l.Cantidad),
                    AplicaIva = p.AplicaIva
                })
                .ToListAsync();

            return Ok(productos);
        }
    }

    public class ProductoDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
        public decimal PrecioVenta { get; set; }
        public bool AplicaIva { get; set; }
        public int Stock { get; set; }           // <-- Nuevo campo
        public bool TieneIva => AplicaIva;       // <-- Para facilitar el frontend
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
        public decimal PrecioVenta { get; set; }
        public bool AplicaIva { get; set; }
        public List<LoteCreateDto> Lotes { get; set; } = new();
    }
    public class LoteCreateDto
    {
        public string NumeroLote { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        // Aceptamos fechas opcionales desde el cliente
        public DateTime? FechaIngreso { get; set; }
        public DateTime? FechaVencimiento { get; set; }
    }
}

