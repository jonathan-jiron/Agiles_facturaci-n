using System.Net.Http.Json;

namespace UI.Services
{
    public class ProductoService
    {
        private readonly HttpClient _http;
        public ProductoService(HttpClient http) => _http = http;

        public async Task<List<ProductoDto>> GetAllAsync()
        {
            var resp = await _http.GetAsync("api/productos");
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<List<ProductoDto>>() ?? new();
        }

        public async Task<(bool ok,string? error)> CreateAsync(ProductoCreate model)
        {
            var resp = await _http.PostAsJsonAsync("api/productos", model);
            if (resp.IsSuccessStatusCode) return (true,null);
            var body = await resp.Content.ReadAsStringAsync();
            return (false,body);
        }

        public class ProductoDto
        {
            public int Id { get; set; }
            public string Codigo { get; set; } = "";
            public string Nombre { get; set; } = "";
            public string? Descripcion { get; set; }
        }

        public class ProductoCreate
        {
            public string Codigo { get; set; } = "";
            public string Nombre { get; set; } = "";
            public string? Descripcion { get; set; }
        }
    }
}