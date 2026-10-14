using Microsoft.AspNetCore.Components;
using R2D2.Blazor.Services;

namespace R2D2.Blazor.Pages;

public partial class Index
{
    [Inject]
    private ApiService ApiService { get; set; } = default!;
}
