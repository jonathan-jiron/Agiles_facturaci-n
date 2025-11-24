using System.Net.Http.Json;
using Domain.Entities;
using UI.DTOs;

// Since this is a Blazor Server/WASM app, we might share DTOs. 
// Let's assume we need to define DTOs or use the ones from Application if referenced.
// Checking UI.csproj references... usually UI references Application.

namespace UI.Services
{
    public class FacturacionService
    {
        private readonly HttpClient _http;

        public FacturacionService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Cliente>> GetClientesAsync()
        {
            return await _http.GetFromJsonAsync<List<Cliente>>("api/clientes") ?? new List<Cliente>();
        }

        public async Task<List<ProductoDto>> GetProductosAsync()
        {
            return await _http.GetFromJsonAsync<List<ProductoDto>>("api/productos") ?? new List<ProductoDto>();
        }

        public async Task<Factura> CrearFacturaAsync(FacturaCreateDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/facturas", dto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }
            return await response.Content.ReadFromJsonAsync<Factura>();
        }

        public async Task<bool> EnviarAlSriAsync(int idFactura)
        {
            var response = await _http.PostAsync($"api/facturacion/{idFactura}", null);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }
            return true;
        }

        public async Task<List<FacturaDto>> ObtenerFacturasAsync()
        {
            var response = await _http.GetAsync("api/facturas");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<FacturaDto>>() ?? new List<FacturaDto>();
        }
    }
}
