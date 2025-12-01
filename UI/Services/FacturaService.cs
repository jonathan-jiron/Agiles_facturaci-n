using System.Net.Http.Json;

namespace UI.Services;

public class FacturaService
{
    private readonly HttpClient _http;

    public FacturaService(HttpClient http)
    {
        _http = http;
    }

    public async Task<int> GetTotalFacturasAsync()
    {
        try
        {
            var facturas = await _http.GetFromJsonAsync<List<FacturaListDto>>("api/facturas");
            return facturas?.Count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<decimal> GetVentasMesActualAsync()
    {
        try
        {
            var facturas = await _http.GetFromJsonAsync<List<FacturaListDto>>("api/facturas");
            if (facturas == null) return 0;

            var mesActual = DateTime.Now.Month;
            var añoActual = DateTime.Now.Year;

            return facturas
                .Where(f => f.Fecha.Month == mesActual && f.Fecha.Year == añoActual)
                .Sum(f => f.Total);
        }
        catch
        {
            return 0;
        }
    }

    public async Task<int> GetFacturasMesActualAsync()
    {
        try
        {
            var facturas = await _http.GetFromJsonAsync<List<FacturaListDto>>("api/facturas");
            if (facturas == null) return 0;

            var mesActual = DateTime.Now.Month;
            var añoActual = DateTime.Now.Year;

            return facturas
                .Where(f => f.Fecha.Month == mesActual && f.Fecha.Year == añoActual)
                .Count();
        }
        catch
        {
            return 0;
        }
    }

    public async Task<decimal> GetCrecimientoVentasAsync()
    {
        try
        {
            var facturas = await _http.GetFromJsonAsync<List<FacturaListDto>>("api/facturas");
            if (facturas == null || !facturas.Any()) return 0;

            var mesActual = DateTime.Now.Month;
            var añoActual = DateTime.Now.Year;
            var mesAnterior = mesActual == 1 ? 12 : mesActual - 1;
            var añoMesAnterior = mesActual == 1 ? añoActual - 1 : añoActual;

            var ventasMesActual = facturas
                .Where(f => f.Fecha.Month == mesActual && f.Fecha.Year == añoActual)
                .Sum(f => f.Total);

            var ventasMesAnterior = facturas
                .Where(f => f.Fecha.Month == mesAnterior && f.Fecha.Year == añoMesAnterior)
                .Sum(f => f.Total);

            if (ventasMesAnterior == 0) return ventasMesActual > 0 ? 100 : 0;

            return Math.Round(((ventasMesActual - ventasMesAnterior) / ventasMesAnterior) * 100, 1);
        }
        catch
        {
            return 0;
        }
    }

    public async Task<FacturaDto?> CrearFacturaAsync(FacturaCreateDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/facturas", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<FacturaDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creando factura: {ex.Message}");
            throw;
        }
    }

    public async Task<FacturaDto?> ObtenerFacturaPorIdAsync(int id)
    {
        try
        {
            return await _http.GetFromJsonAsync<FacturaDto>($"api/facturas/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<FacturaListDto>> ListarFacturasAsync()
    {
        try
        {
            var facturas = await _http.GetFromJsonAsync<List<FacturaListDto>>("api/facturas");
            return facturas ?? new List<FacturaListDto>();
        }
        catch
        {
            return new List<FacturaListDto>();
        }
    }

    public async Task<bool> EnviarFacturaPorEmailAsync(int facturaId, string email)
    {
        try
        {
            // Placeholder - implementar cuando el backend tenga el endpoint
            var response = await _http.PostAsJsonAsync($"api/facturas/{facturaId}/enviar-email", new { email });
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}


public class FacturaListDto
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "Generada";
    public string? ClienteNombre { get; set; }
}

public class FacturaCreateDto
{
    public int ClienteId { get; set; }
    public List<DetalleFacturaCreateDto> Detalles { get; set; } = new();
}

public class DetalleFacturaCreateDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}

public class FacturaDto
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public int ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public string? ClienteIdentificacion { get; set; }
    public string? ClienteEmail { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "Generada";
    public List<DetalleFacturaDto> Detalles { get; set; } = new();
}

public class DetalleFacturaDto
{
    public int ProductoId { get; set; }
    public string? ProductoNombre { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
}

