using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            try
            {
                var clientes = await _context.Clientes.ToListAsync();
                return Ok(clientes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: api/clientes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound(new { message = "Cliente no encontrado" });
            }

            return cliente;
        }

        // POST: api/clientes
        [HttpPost]
        public async Task<ActionResult<Cliente>> PostCliente(Cliente cliente)
        {
            // Validaciones del SRI
            var validationResult = ValidarClienteSRI(cliente);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { message = validationResult.ErrorMessage });
            }

            // Verificar si ya existe un cliente con la misma identificación
            var existeCliente = await _context.Clientes
                .AnyAsync(c => c.CedulaRuc == cliente.CedulaRuc && c.Id != cliente.Id);

            if (existeCliente)
            {
                return BadRequest(new { message = "Ya existe un cliente con esta identificación" });
            }

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, cliente);
        }

        // PUT: api/clientes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCliente(int id, Cliente cliente)
        {
            if (id != cliente.Id)
            {
                return BadRequest(new { message = "El ID no coincide" });
            }

            // Validaciones del SRI
            var validationResult = ValidarClienteSRI(cliente);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { message = validationResult.ErrorMessage });
            }

            // Verificar si ya existe otro cliente con la misma identificación
            var existeCliente = await _context.Clientes
                .AnyAsync(c => c.CedulaRuc == cliente.CedulaRuc && c.Id != cliente.Id);

            if (existeCliente)
            {
                return BadRequest(new { message = "Ya existe otro cliente con esta identificación" });
            }

            _context.Entry(cliente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClienteExists(id))
                {
                    return NotFound(new { message = "Cliente no encontrado" });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/clientes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound(new { message = "Cliente no encontrado" });
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }

        private ValidationResult ValidarClienteSRI(Cliente cliente)
        {
            // Validar Tipo de Identificación
            if (string.IsNullOrWhiteSpace(cliente.TipoIdentificacion))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "El tipo de identificación es requerido" };
            }

            if (!new[] { "CEDULA", "RUC", "PASAPORTE" }.Contains(cliente.TipoIdentificacion))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "Tipo de identificación inválido" };
            }

            // Validar según tipo
            switch (cliente.TipoIdentificacion)
            {
                case "CEDULA":
                    if (string.IsNullOrWhiteSpace(cliente.CedulaRuc) || cliente.CedulaRuc.Length != 10 || !Regex.IsMatch(cliente.CedulaRuc, @"^\d{10}$"))
                    {
                        return new ValidationResult { IsValid = false, ErrorMessage = "La cédula debe tener exactamente 10 dígitos" };
                    }
                    if (!ValidarCedulaEcuatoriana(cliente.CedulaRuc))
                    {
                        return new ValidationResult { IsValid = false, ErrorMessage = "La cédula no es válida según el algoritmo del SRI" };
                    }
                    break;

                case "RUC":
                    if (string.IsNullOrWhiteSpace(cliente.CedulaRuc) || cliente.CedulaRuc.Length != 13 || !Regex.IsMatch(cliente.CedulaRuc, @"^\d{13}$"))
                    {
                        return new ValidationResult { IsValid = false, ErrorMessage = "El RUC debe tener exactamente 13 dígitos" };
                    }
                    if (!cliente.CedulaRuc.EndsWith("001"))
                    {
                        return new ValidationResult { IsValid = false, ErrorMessage = "El RUC debe terminar en 001" };
                    }
                    break;

                case "PASAPORTE":
                    if (string.IsNullOrWhiteSpace(cliente.CedulaRuc) || cliente.CedulaRuc.Length < 5 || cliente.CedulaRuc.Length > 20)
                    {
                        return new ValidationResult { IsValid = false, ErrorMessage = "El pasaporte debe tener entre 5 y 20 caracteres" };
                    }
                    break;
            }

            // Validar Teléfono
            if (string.IsNullOrWhiteSpace(cliente.Telefono) || cliente.Telefono.Length != 10 || !Regex.IsMatch(cliente.Telefono, @"^\d{10}$"))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "El teléfono debe tener exactamente 10 dígitos" };
            }

            // Validar Dirección
            if (string.IsNullOrWhiteSpace(cliente.Direccion))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "La dirección es obligatoria para facturación electrónica" };
            }

            // Validar Email
            if (string.IsNullOrWhiteSpace(cliente.Correo) || !Regex.IsMatch(cliente.Correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "El correo electrónico no es válido" };
            }

            return new ValidationResult { IsValid = true };
        }

        private bool ValidarCedulaEcuatoriana(string cedula)
        {
            if (cedula.Length != 10) return false;

            int[] coeficientes = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
            int suma = 0;
            int digitoVerificador = int.Parse(cedula[9].ToString());

            for (int i = 0; i < 9; i++)
            {
                int digito = int.Parse(cedula[i].ToString()) * coeficientes[i];
                if (digito > 9) digito -= 9;
                suma += digito;
            }

            int resultado = suma % 10 == 0 ? 0 : 10 - (suma % 10);
            return resultado == digitoVerificador;
        }

        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public string ErrorMessage { get; set; } = string.Empty;
        }
    }
}
