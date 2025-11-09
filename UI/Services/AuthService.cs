using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace UI.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private readonly AuthStateProvider _stateProvider;

    public AuthService(HttpClient http, IJSRuntime js, AuthenticationStateProvider authState)
    {
        _http = http;
        _js = js;
        _stateProvider = (AuthStateProvider)authState;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var resp = await _http.PostAsJsonAsync("api/auth/login", new { username, password });
        if (!resp.IsSuccessStatusCode) return false;

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var token = doc.RootElement.GetProperty("token").GetString();
        if (string.IsNullOrWhiteSpace(token)) return false;

        await _js.InvokeVoidAsync("localStorage.setItem", "authToken", token);
        await _stateProvider.MarkUserAuthenticatedAsync(token);
        return true;
    }

    public Task<bool> Login(string username, string password) => LoginAsync(username, password);

    public async Task LogoutAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");
        await _stateProvider.MarkUserLoggedOutAsync();
    }

    public async Task<bool> IsAuthenticated()
    {
        var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");
        return !string.IsNullOrWhiteSpace(token);
    }

    public async Task<UsuarioInfo?> GetCurrentUser()
    {
        var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");
        if (string.IsNullOrWhiteSpace(token)) return null;

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        System.IdentityModel.Tokens.Jwt.JwtSecurityToken? jwt;
        try { jwt = handler.ReadJwtToken(token); } catch { return null; }

        var username = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == "unique_name")?.Value ?? "";
        var rol = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value ?? "";
        return new UsuarioInfo { Username = username, Rol = rol };
    }
}