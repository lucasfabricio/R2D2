using System.Net;
using Microsoft.AspNetCore.Components;
using Blazored.LocalStorage;

namespace R2D2.Blazor.Services;

public class AuthInterceptorHandler : DelegatingHandler
{
    private readonly NavigationManager _navigationManager;
    private readonly ILocalStorageService _localStorage;

    public AuthInterceptorHandler(NavigationManager navigationManager, ILocalStorageService localStorage)
    {
        _navigationManager = navigationManager;
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await _localStorage.RemoveItemAsync("authToken");
            _navigationManager.NavigateTo("/login");
        }

        return response;
    }
}
