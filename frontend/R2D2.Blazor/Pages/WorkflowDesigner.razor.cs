using Microsoft.AspNetCore.Components;
using R2D2.Blazor.Models;
using R2D2.Blazor.Services;

namespace R2D2.Blazor.Pages;

public partial class WorkflowDesigner
{
    [Parameter] public Guid WorkflowId { get; set; }

    [Inject]
    private ApiService ApiService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private Workflow? workflow;
    private List<BusinessRule> availableRules = new();
    private string searchQuery = "";
    private bool isLoading = true;

    private IEnumerable<BusinessRule> filteredRules => 
        string.IsNullOrWhiteSpace(searchQuery) 
            ? availableRules 
            : availableRules.Where(r => r.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase));

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        isLoading = true;
        try
        {
            workflow = await ApiService.GetWorkflowAsync(WorkflowId);
            availableRules = await ApiService.GetRulesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading workflow designer: {ex.Message}");
        }
        isLoading = false;
    }

    private async Task AddStep(Guid ruleId)
    {
        if (workflow == null) return;
        
        var success = await ApiService.AddStepToWorkflowAsync(WorkflowId, ruleId, workflow.Steps.Count);
        if (success)
        {
            await RefreshWorkflow();
        }
    }

    private async Task RemoveStep(Guid stepId)
    {
        if (workflow == null) return;

        var success = await ApiService.RemoveStepFromWorkflowAsync(WorkflowId, stepId);
        if (success)
        {
            await RefreshWorkflow();
        }
    }

    private async Task RefreshWorkflow()
    {
        var updatedWorkflow = await ApiService.GetWorkflowAsync(WorkflowId);
        if (updatedWorkflow != null)
        {
            workflow = updatedWorkflow;
            StateHasChanged();
        }
    }
}
