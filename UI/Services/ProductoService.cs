using System.Net.Http.Json;
using Domain.Entities;

namespace UI.Services
{
    public class ProductoService
    {
        private readonly HttpClient _http;
        public ProductoService(HttpClient http) => _http = http;

        public async Task<List<Producto>> GetAllAsync()
        {
            var resp = await _http.GetAsync("api/productos");
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<List<Producto>>() ?? new();
        }

        public async Task<(bool ok,string? error)> CreateAsync(ProductoCreate model)
        {
            var resp = await _http.PostAsJsonAsync("api/productos", model);
            if (resp.IsSuccessStatusCode) return (true,null);
            var body = await resp.Content.ReadAsStringAsync();
            return (false,body);
        }

        public async Task<int> GetProductosCountAsync()
        {
            var productos = await GetAllAsync();
            return productos.Count;
        }

        public async Task<List<EventoActividad>> GetActividadRecienteAsync()
        {
            var resp = await _http.GetAsync("api/actividad");
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<List<EventoActividad>>() ?? new();
        }

        public class ProductoCreate
        {
            public string Codigo { get; set; } = "";
            public string Nombre { get; set; } = "";
            public string? Descripcion { get; set; }
        }
    }
}