using R2D2.Blazor.Models;
using R2D2.Blazor.Services;
using Microsoft.AspNetCore.Components;
using BlazorBootstrap;

namespace R2D2.Blazor.Pages;

public partial class Forms : ComponentBase
{
    private List<DynamicForm> forms = new();
    private List<Workflow> workflows = new();
    private bool isLoading = true;
    private Modal formModal = default!;
    // private bool showFormModal; // Removed
    private DynamicForm? editingForm;

    protected override async Task OnInitializedAsync()
    {
        await LoadForms();
    }

    private async Task LoadForms()
    {
        isLoading = true;
        forms = await ApiService.GetFormsAsync();
        workflows = await ApiService.GetWorkflowsAsync();
        isLoading = false;
    }

    private async Task OpenCreateModal()
    {
        editingForm = new DynamicForm();
        await formModal.ShowAsync();
    }

    private async Task OpenEditModal(DynamicForm form)
    {
        editingForm = new DynamicForm
        {
            Id = form.Id,
            Name = form.Name,
            Description = form.Description,
            WorkflowId = form.WorkflowId,
        };
        await formModal.ShowAsync();
    }

    private async Task CloseModal()
    {
        await formModal.HideAsync();
        editingForm = null;
    }

    private async Task SaveForm()
    {
        if (editingForm == null) return;

        if (editingForm.Id == Guid.Empty)
        {
            await ApiService.CreateFormAsync(editingForm);
        }
        else
        {
            await ApiService.UpdateFormAsync(editingForm.Id, editingForm);
        }

        await LoadForms();
        await CloseModal();
    }

    private async Task DeleteForm(DynamicForm form)
    {
        await ApiService.DeleteFormAsync(form.Id);
        await LoadForms();
    }

    private void OpenDesigner(DynamicForm form)
    {
        Navigation.NavigateTo($"/form-builder/{form.Id}");
    }

    private void ViewForm(DynamicForm form)
    {
        Navigation.NavigateTo($"/form/{form.Id}");
    }
}
