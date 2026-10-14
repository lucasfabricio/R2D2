using Microsoft.AspNetCore.Components;
using R2D2.Blazor.Services;

namespace R2D2.Blazor.Pages;

public partial class Login
{
    [Inject]
    private ApiService ApiService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private string _username = "";
    private string _password = "";
    private string? _errorMessage;
    private bool _isLoggingIn;

    private async Task HandleLogin()
    {
        if (string.IsNullOrWhiteSpace(_username) || string.IsNullOrWhiteSpace(_password))
        {
            _errorMessage = "Por favor, insira o usuário e a senha.";
            return;
        }

        _isLoggingIn = true;
        _errorMessage = null;

        try
        {
            var result = await ApiService.LoginAsync(_username, _password);
            if (result != null)
            {
                Navigation.NavigateTo("/");
            }
            else
            {
                _errorMessage = "Usuário ou senha inválidos.";
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Ocorreu um erro: {ex.Message}";
        }
        finally
        {
            _isLoggingIn = false;
        }
    }
}
