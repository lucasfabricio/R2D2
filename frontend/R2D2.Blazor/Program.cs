using Blazored.LocalStorage;
using R2D2.Blazor.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazorBootstrap();

// Add HttpClient and API Service
builder.Services.AddHttpClient("R2D2.API", client =>
{
    client.BaseAddress = new Uri("https://localhost:5051/api/");
})
.AddHttpMessageHandler<AuthInterceptorHandler>();

builder.Services.AddScoped<ApiService>(sp => 
{
    var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var client = clientFactory.CreateClient("R2D2.API");
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var authStateProvider = sp.GetRequiredService<AuthenticationStateProvider>();
    return new ApiService(client, localStorage, authStateProvider);
});

builder.Services.AddTransient<AuthInterceptorHandler>();

// Add Authorization Core
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
