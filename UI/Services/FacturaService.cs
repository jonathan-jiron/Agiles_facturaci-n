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

    public async Task<EnviarEmailResult?> EnviarFacturaPorEmailAsync(int id)
    {
        try
        {
            var resp = await _http.PostAsync($"api/facturas/{id}/enviar-email", null);
            if (resp.IsSuccessStatusCode)
            {
                var resultado = await resp.Content.ReadFromJsonAsync<EnviarEmailResult>();
                return resultado ?? new EnviarEmailResult { Exito = true };
            }
            else
            {
                var error = await resp.Content.ReadAsStringAsync();
                return new EnviarEmailResult { Exito = false, Error = error };
            }
        }
        catch (Exception ex)
        {
            return new EnviarEmailResult { Exito = false, Error = ex.Message };
        }
    }
    
    public class EnviarEmailResult
    {
        public bool Exito { get; set; }
        public string? Mensaje { get; set; }
        public string? Error { get; set; }
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

    public async Task<(decimal crecimiento, bool hayDatosAnterior)> GetCrecimientoVentasAsync()
    {
        try
        {
            var resultado = await _http.GetFromJsonAsync<CrecimientoResult>("api/facturas/crecimiento-ventas");
            return (resultado?.crecimiento ?? 0, resultado?.hayDatosAnterior ?? false);
        }
        catch
        {
            return (0, false);
        }
    }

    public async Task<(decimal crecimiento, bool hayDatosAnterior)> GetCrecimientoFacturasAsync()
    {
        try
        {
            var resultado = await _http.GetFromJsonAsync<CrecimientoResult>("api/facturas/crecimiento-facturas");
            return (resultado?.crecimiento ?? 0, resultado?.hayDatosAnterior ?? false);
        }
        catch
        {
            return (0, false);
        }
    }

    public async Task<(decimal crecimiento, bool hayDatosAnterior)> GetCrecimientoClientesAsync()
    {
        try
        {
            var resultado = await _http.GetFromJsonAsync<CrecimientoResult>("api/facturas/crecimiento-clientes");
            return (resultado?.crecimiento ?? 0, resultado?.hayDatosAnterior ?? false);
        }
        catch
        {
            return (0, false);
        }
    }

    public async Task<(decimal crecimiento, bool hayDatosAnterior)> GetCrecimientoProductosAsync()
    {
        try
        {
            var resultado = await _http.GetFromJsonAsync<CrecimientoResult>("api/facturas/crecimiento-productos");
            return (resultado?.crecimiento ?? 0, resultado?.hayDatosAnterior ?? false);
        }
        catch
        {
            return (0, false);
        }
    }

    public class CrecimientoResult
    {
        public decimal crecimiento { get; set; }
        public bool hayDatosAnterior { get; set; }
    }

    public async Task<List<FacturaListDto>> GetFacturasRecientesAsync(int cantidad = 5)
    {
        try
        {
            var facturas = await ListarFacturasAsync();
            return facturas.Take(cantidad).ToList();
        }
        catch
        {
            return new List<FacturaListDto>();
        }
    }

    public async Task<FacturaConsultaDto?> ConsultarFacturaAsync(int id)
    {
        return await _http.GetFromJsonAsync<FacturaConsultaDto>($"api/facturas/{id}");
    }

    public async Task<FirmarFacturaResult?> FirmarFacturaAsync(int id)
    {
        try
        {
            var resp = await _http.PostAsync($"api/facturas/{id}/firmar", null);
            if (resp.IsSuccessStatusCode)
            {
                var resultado = await resp.Content.ReadFromJsonAsync<FirmarFacturaResult>();
                return resultado ?? new FirmarFacturaResult { Exito = true };
            }
            else
            {
                var error = await resp.Content.ReadAsStringAsync();
                return new FirmarFacturaResult { Exito = false, Error = error };
            }
        }
        catch (Exception ex)
        {
            return new FirmarFacturaResult { Exito = false, Error = ex.Message };
        }
    }
    
    public class FirmarFacturaResult
    {
        public bool Exito { get; set; }
        public int? Id { get; set; }
        public string? Numero { get; set; }
        public string? Estado { get; set; }
        public string? EstadoSri { get; set; }
        public string? ClaveAcceso { get; set; }
        public string? Error { get; set; }
    }

    public async Task<EmitirSriResult?> EmitirSriAsync(int id)
    {
        try
        {
            var resp = await _http.PostAsync($"api/facturas/{id}/emitir-sri", null);
            if (resp.IsSuccessStatusCode)
            {
                return await resp.Content.ReadFromJsonAsync<EmitirSriResult>();
            }
            else
            {
                var error = await resp.Content.ReadAsStringAsync();
                return new EmitirSriResult { Error = error };
            }
        }
        catch (Exception ex)
        {
            return new EmitirSriResult { Error = ex.Message };
        }
    }
}

public class EmitirSriResult
{
    public int Id { get; set; }
    public string? ClaveAcceso { get; set; }
    public string? EstadoSri { get; set; }
    public string? NumeroAutorizacion { get; set; }
    public DateTime? FechaAutorizacion { get; set; }
    public string? MensajesSri { get; set; }
    public string? Error { get; set; }
}