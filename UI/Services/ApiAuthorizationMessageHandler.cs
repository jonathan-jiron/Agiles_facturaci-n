using System.Net.Http.Headers;
using Microsoft.JSInterop;

public class ApiAuthorizationMessageHandler : DelegatingHandler
{
    private readonly IJSRuntime _js;
    public ApiAuthorizationMessageHandler(IJSRuntime js) => _js = js;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var path = request.RequestUri?.AbsolutePath?.ToLowerInvariant() ?? "";
        if (!path.Contains("/api/auth/login"))
        {
            var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return await base.SendAsync(request, ct);
    }
}