using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Domain.Entities;

namespace UI.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private readonly AuthStateProvider _stateProvider;
    private readonly ProductoService _productoService;

    public AuthService(HttpClient http, IJSRuntime js, AuthenticationStateProvider authState, ProductoService productoService)
    {
        _http = http;
        _js = js;
        _stateProvider = (AuthStateProvider)authState;
        _productoService = productoService;
    }

    public async Task<(bool ok, string? error)> LoginAsync(string username, string password)
    {
        username = username?.Trim() ?? "";
        password = password ?? "";

        var resp = await _http.PostAsJsonAsync("api/auth/login", new { username, password });

        if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest)
            return (false, "Datos inválidos");

        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return (false, "Usuario o contraseña incorrectos");

        if (!resp.IsSuccessStatusCode)
            return (false, $"Error servidor ({(int)resp.StatusCode})");

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("token", out var tokenProp))
            return (false, "Respuesta sin token");

        var token = tokenProp.GetString();
        if (string.IsNullOrWhiteSpace(token))
            return (false, "Token inválido");

        await _js.InvokeVoidAsync("localStorage.setItem", "authToken", token);
        await _stateProvider.MarkUserAuthenticatedAsync(token);
        return (true, null);
    }

    public async Task LogoutAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");
        await _stateProvider.MarkUserLoggedOutAsync();
    }

    private static bool TokenExpired(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwt;
        try { jwt = handler.ReadJwtToken(token); } catch { return true; }
        return jwt.ValidTo <= DateTime.UtcNow;
    }

    public async Task<bool> IsAuthenticated()
    {
        var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");
        return !string.IsNullOrWhiteSpace(token) && !TokenExpired(token);
    }

    public async Task<UsuarioInfo?> GetCurrentUser()
    {
        var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");
        if (string.IsNullOrWhiteSpace(token) || TokenExpired(token)) return null;

        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwt;
        try { jwt = handler.ReadJwtToken(token); } catch { return null; }

        var username = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == "unique_name")?.Value ?? "";
        var rol = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value ?? "";
        return new UsuarioInfo { Username = username, Rol = rol };
    }

    public async Task<int> GetProductosCountAsync()
    {
        var productos = await _productoService.GetAllAsync();
        return productos.Count;
    }

    public async Task<int> GetClientesCountAsync()
    {
        var resp = await _http.GetAsync("api/clientes");
        resp.EnsureSuccessStatusCode();
        var clientes = await resp.Content.ReadFromJsonAsync<List<Cliente>>() ?? new();
        return clientes.Count;
    }

    public async Task<List<EventoActividad>> GetActividadRecienteAsync()
    {
        var resp = await _http.GetAsync("api/actividad");
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<List<EventoActividad>>() ?? new();
    }
}