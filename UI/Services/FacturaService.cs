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
}

public class FacturaListDto
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
}
