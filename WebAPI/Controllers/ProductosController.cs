using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Domain.Entities;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ProductosController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var productos = await _context.Productos.ToListAsync();
            return Ok(productos);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Producto producto)
        {
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAll), new { id = producto.Id }, producto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Producto producto)
        {
            if (id != producto.Id) return BadRequest();
            _context.Entry(producto).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // 🔹 Lógica FIFO: salida de productos
        [HttpPost("{id}/salida")]
        public async Task<IActionResult> SalidaProducto(int id, int cantidad)
        {
            var lotes = await _context.Lotes
                .Where(l => l.ProductoId == id && l.Cantidad > 0)
                .OrderBy(l => l.FechaIngreso) // FIFO: primero en entrar, primero en salir
                .ToListAsync();

            if (!lotes.Any()) return NotFound("No hay lotes disponibles.");

            int restante = cantidad;
            foreach (var lote in lotes)
            {
                if (restante <= 0) break;
                int usar = Math.Min(lote.Cantidad, restante);
                lote.Cantidad -= usar;
                restante -= usar;
            }

            await _context.SaveChangesAsync();
            return Ok($"Salida de {cantidad} unidades del producto {id} realizada con FIFO.");
        }
    }
}

