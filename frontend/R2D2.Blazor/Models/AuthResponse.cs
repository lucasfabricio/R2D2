namespace R2D2.Blazor.Models;

public class AuthResponse
{
    public string Token { get; set; } = "";
    public string Username { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
}
