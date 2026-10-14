using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace R2D2.Blazor.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private bool _isInteropReady;

    public CustomAuthenticationStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    /// <summary>
    /// Whether JS Interop has been initialized and the auth state is reliable.
    /// Before this is true, the auth state should be treated as "loading/unknown".
    /// </summary>
    public bool IsInteropReady => _isInteropReady;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Before JS Interop is ready, return anonymous — but callers should check
        // IsInteropReady to know if this is a real "not authenticated" or just "not yet known"
        if (!_isInteropReady)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        string? token = null;

        try 
        {
            token = await _localStorage.GetItemAsStringAsync("authToken");
        }
        catch (InvalidOperationException) 
        {
            // JS Interop not available — shouldn't happen after MarkAsReady, but be safe
        }

        var identity = new ClaimsIdentity();

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                token = token.Trim('"'); // Blazored sometimes returns quotes
                identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
            }
            catch
            {
                await _localStorage.RemoveItemAsync("authToken");
            }
        }

        var user = new ClaimsPrincipal(identity);
        var state = new AuthenticationState(user);

        return state;
    }

    /// <summary>
    /// Called from OnAfterRenderAsync when JS Interop becomes available.
    /// Re-evaluates the authentication state from localStorage.
    /// </summary>
    public async Task MarkAsReadyAsync()
    {
        if (_isInteropReady) return;
        
        _isInteropReady = true;

        // Re-read auth state now that localStorage is accessible
        var state = await GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(state));
    }

    public void NotifyUserAuthentication(string token)
    {
        try
        {
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Auth] Exception in NotifyUserAuthentication: {ex}");
        }
    }

    public void NotifyUserLogout()
    {
        var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        NotifyAuthenticationStateChanged(authState);
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
