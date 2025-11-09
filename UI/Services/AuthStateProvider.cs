using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;

namespace UI.Services;

public class AuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    private ClaimsPrincipal _current = new(new ClaimsIdentity());

    public AuthStateProvider(IJSRuntime js) => _js = js;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");
        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        _current = BuildPrincipalFromToken(token);
        return new AuthenticationState(_current);
    }

    public Task MarkUserAuthenticatedAsync(string token)
    {
        _current = BuildPrincipalFromToken(token);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_current)));
        return Task.CompletedTask;
    }

    public Task MarkUserLoggedOutAsync()
    {
        _current = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_current)));
        return Task.CompletedTask;
    }

    private ClaimsPrincipal BuildPrincipalFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken? jwt = null;
        try { jwt = handler.ReadJwtToken(token); } catch { }

        if (jwt == null)
            return new ClaimsPrincipal(new ClaimsIdentity());

        return new ClaimsPrincipal(new ClaimsIdentity(jwt.Claims, "jwt"));
    }
}