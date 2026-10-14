using Microsoft.AspNetCore.Components;
using R2D2.Blazor.Models;
using R2D2.Blazor.Services;
using BlazorBootstrap;

namespace R2D2.Blazor.Pages;

public partial class Workflows
{
    [Inject]
    private ApiService ApiService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private List<Workflow> workflows = new();
    private bool isLoading = true;
    private Modal workflowModal = default!;
    // private bool showModal = false; // Removed
    private bool isEditing = false;
    private Workflow currentWorkflow = new();

    protected override async Task OnInitializedAsync() => await LoadWorkflows();

    private async Task LoadWorkflows()
    {
        isLoading = true;
        try { workflows = await ApiService.GetWorkflowsAsync(); }
        catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
        isLoading = false;
    }

    private async Task ShowCreateModal()
    {
        currentWorkflow = new Workflow { FlowJson = "{}", IsActive = true };
        isEditing = false;
        await workflowModal.ShowAsync();
    }

    private async Task EditWorkflow(Workflow workflow)
    {
        currentWorkflow = new Workflow
        {
            Id = workflow.Id, Name = workflow.Name, Description = workflow.Description,
            FlowJson = workflow.FlowJson, IsActive = workflow.IsActive
        };
        isEditing = true;
        await workflowModal.ShowAsync();
    }

    private async Task CloseModal() { await workflowModal.HideAsync(); currentWorkflow = new Workflow(); }

    private async Task SaveWorkflow()
    {
        try
        {
            if (isEditing) await ApiService.UpdateWorkflowAsync(currentWorkflow.Id, currentWorkflow);
            else await ApiService.CreateWorkflowAsync(currentWorkflow);
            CloseModal();
            await LoadWorkflows();
        }
        catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
    }

    private async Task ToggleWorkflow(Workflow workflow)
    {
        try
        {
            workflow.IsActive = !workflow.IsActive;
            await ApiService.UpdateWorkflowAsync(workflow.Id, workflow);
            await LoadWorkflows();
        }
        catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
    }

    private async Task DeleteWorkflow(Guid id)
    {
        try { await ApiService.DeleteWorkflowAsync(id); await LoadWorkflows(); }
        catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
    }

    private void OpenWorkflowBuilder(Guid id) => NavigationManager.NavigateTo($"/workflowdesigner/{id}");

    private async Task ExecuteWorkflow(Guid id)
    {
        try { await ApiService.ExecuteWorkflowAsync(id, new Dictionary<string, object?>()); }
        catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
    }
}
