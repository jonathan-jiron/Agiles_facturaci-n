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
            try
            {
                var prod = await _db.Productos.Include(p => p.Lotes).FirstOrDefaultAsync(p => p.Id == id);
                if (prod is null) return NotFound(new { error = "Producto no encontrado" });

                // Validar que haya al menos un lote
                if (dto.Lotes == null || !dto.Lotes.Any())
                {
                    return BadRequest(new { error = "Debe haber al menos un lote para el producto" });
                }

                // Validar números de lote únicos en el DTO
                var numerosLoteDuplicados = dto.Lotes
                    .GroupBy(l => l.NumeroLote)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();
                
                if (numerosLoteDuplicados.Any())
                {
                    return BadRequest(new { error = $"Hay números de lote duplicados: {string.Join(", ", numerosLoteDuplicados)}" });
                }

                prod.Codigo = dto.Codigo;
                prod.Nombre = dto.Nombre;
                prod.Descripcion = dto.Descripcion;
                prod.PrecioVenta = dto.PrecioVenta;
                prod.AplicaIva = dto.AplicaIva;

                // Verificar qué lotes están siendo usados en facturas (no se pueden eliminar)
                // Primero obtener los IDs de los lotes del producto en memoria
                var idsLotesProducto = prod.Lotes.Select(l => l.Id).ToList();
                
                // Luego buscar en la BD qué lotes están siendo usados
                var lotesEnUso = await _db.DetallesFactura
                    .Where(d => d.LoteId.HasValue && idsLotesProducto.Contains(d.LoteId.Value))
                    .Select(d => d.LoteId.Value)
                    .Distinct()
                    .ToListAsync();

                // Obtener números de lote que vienen del DTO
                var numerosLoteNuevos = dto.Lotes.Select(l => l.NumeroLote).ToList();

                // PRIMERO: Eliminar solo los lotes que NO están en uso y NO están en la lista nueva
                // Esto debe hacerse ANTES de agregar nuevos lotes para evitar conflictos
                var lotesAEliminar = prod.Lotes
                    .Where(l => !lotesEnUso.Contains(l.Id) && !numerosLoteNuevos.Contains(l.NumeroLote))
                    .ToList();
                
                // Guardar los IDs de los lotes que vamos a eliminar para evitar conflictos
                var idsAEliminar = lotesAEliminar.Select(l => l.Id).ToList();
                
                // Eliminar los lotes
                foreach (var lote in lotesAEliminar)
                {
                    prod.Lotes.Remove(lote);
                    _db.Lotes.Remove(lote);
                }
                
                // Guardar cambios parciales para que los lotes eliminados ya no existan en la BD
                await _db.SaveChangesAsync();

                // SEGUNDO: Agregar o actualizar lotes nuevos
                foreach (var loteDto in dto.Lotes)
                {
                    // Validar que el número de lote no esté vacío
                    if (string.IsNullOrWhiteSpace(loteDto.NumeroLote))
                    {
                        return BadRequest(new { error = "El número de lote no puede estar vacío" });
                    }

                    // Buscar si ya existe un lote con este número en este producto
                    // (después de la eliminación, solo quedan los que no eliminamos)
                    var loteExistente = prod.Lotes
                        .FirstOrDefault(l => l.NumeroLote == loteDto.NumeroLote && !idsAEliminar.Contains(l.Id));
                    
                    if (loteExistente != null)
                    {
                        // Actualizar lote existente (solo si no está en uso)
                        if (!lotesEnUso.Contains(loteExistente.Id))
                        {
                            loteExistente.Cantidad = loteDto.Cantidad;
                            loteExistente.PrecioUnitario = loteDto.PrecioUnitario;
                            if (loteDto.FechaIngreso.HasValue)
                                loteExistente.FechaIngreso = loteDto.FechaIngreso.Value;
                            loteExistente.FechaVencimiento = loteDto.FechaVencimiento;
                        }
                    }
                    else
                    {
                        // Verificar si el número de lote ya existe en otro producto
                        var existeEnOtroProducto = await _db.Lotes
                            .AnyAsync(l => l.NumeroLote == loteDto.NumeroLote && l.ProductoId != id);
                        
                        if (existeEnOtroProducto)
                        {
                            return BadRequest(new { error = $"El número de lote '{loteDto.NumeroLote}' ya existe en otro producto" });
                        }

                        // Crear nuevo lote
                        prod.Lotes.Add(new Lote
                        {
                            NumeroLote = loteDto.NumeroLote,
                            Cantidad = loteDto.Cantidad,
                            PrecioUnitario = loteDto.PrecioUnitario,
                            FechaIngreso = loteDto.FechaIngreso ?? DateTime.Now,
                            FechaVencimiento = loteDto.FechaVencimiento
                        });
                    }
                }

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
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                // Capturar errores de base de datos
                var innerException = ex.InnerException?.Message ?? ex.Message;
                
                if (innerException.Contains("UNIQUE") || innerException.Contains("duplicate"))
                {
                    return BadRequest(new { error = "El número de lote ya existe. Debe ser único." });
                }
                
                if (innerException.Contains("FOREIGN KEY") || innerException.Contains("constraint"))
                {
                    return BadRequest(new { error = "No se puede eliminar un lote que está siendo usado en una factura." });
                }

                return StatusCode(500, new { error = "Error al guardar en la base de datos", details = innerException });
            }
            catch (Exception ex)
            {
                // Log del error completo para debugging
                var errorMessage = ex.Message;
                var stackTrace = ex.StackTrace;
                var innerException = ex.InnerException?.Message;
                
                // Construir mensaje más descriptivo
                var mensaje = $"Error interno al actualizar el producto: {errorMessage}";
                if (!string.IsNullOrEmpty(innerException))
                {
                    mensaje += $" | Detalle: {innerException}";
                }
                
                return StatusCode(500, new { 
                    error = mensaje, 
                    details = errorMessage,
                    innerException = innerException,
                    type = ex.GetType().Name
                });
            }
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
                .Include(p => p.Lotes)
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

            // Filtrar productos con stock > 0 después de cargar en memoria
            var productosConStock = productos.Where(p => p.Stock > 0).ToList();

            return Ok(productosConStock);
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

