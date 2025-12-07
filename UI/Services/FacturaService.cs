using System.Net.Http.Json;
using Application.DTOs;

namespace UI.Services;

public class FacturaService
{
    private readonly HttpClient _http;

    public FacturaService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<FacturaListDto>> ListarFacturasAsync()
    {
        return await _http.GetFromJsonAsync<List<FacturaListDto>>("api/facturas") ?? new();
    }

    public async Task<FacturaDto?> ObtenerFacturaPorIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<FacturaDto>($"api/facturas/{id}");
    }

    public async Task<FacturaDto?> CrearFacturaAsync(FacturaCreateDto dto)
    {
        var resp = await _http.PostAsJsonAsync("api/facturas", dto);
        return await resp.Content.ReadFromJsonAsync<FacturaDto>();
    }

    public async Task<byte[]?> DescargarPDFAsync(int id)
    {
        return await _http.GetByteArrayAsync($"api/facturas/{id}/pdf");
    }

    public async Task<bool> EnviarFacturaPorEmailAsync(int id, string email)
    {
        var resp = await _http.PostAsync($"api/facturas/{id}/enviar-email?email={email}", null);
        return resp.IsSuccessStatusCode;
    }

    public async Task<FacturaDto?> GuardarBorradorAsync(FacturaCreateDto dto)
    {
        var resp = await _http.PostAsJsonAsync("api/facturas/borrador", dto);
        return await resp.Content.ReadFromJsonAsync<FacturaDto>();
    }

    // Métodos para el dashboard
    public async Task<int> GetTotalFacturasAsync()
    {
        return await _http.GetFromJsonAsync<int>("api/facturas/total");
    }

    public async Task<int> GetFacturasMesActualAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<int>("api/facturas/mes-actual");
        }
        catch
        {
            return 0;
        }
    }

    public async Task<decimal> GetVentasMesActualAsync()
    {
        return await _http.GetFromJsonAsync<decimal>("api/facturas/ventas-mes-actual");
    }

    public async Task<decimal> GetCrecimientoVentasAsync()
    {
        return await _http.GetFromJsonAsync<decimal>("api/facturas/crecimiento-ventas");
    }

    public async Task<FacturaConsultaDto?> ConsultarFacturaAsync(int id)
    {
        return await _http.GetFromJsonAsync<FacturaConsultaDto>($"api/facturas/{id}");
    }

    public async Task<bool> FirmarFacturaAsync(int id)
    {
        var resp = await _http.PostAsync($"api/facturas/{id}/firmar", null);
        return resp.IsSuccessStatusCode;
    }
}