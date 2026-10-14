using R2D2.Blazor.Models;
using R2D2.Blazor.Services;
using Microsoft.AspNetCore.Components;

namespace R2D2.Blazor.Pages;

public partial class FormBuilder : ComponentBase
{
    [Parameter] public Guid FormId { get; set; }

    private DynamicForm? form;
    private List<DynamicList> availableLists = new();
    private bool isLoading = true;

    private bool showAddFieldModal;
    private FormField? newField;

    private Guid? selectedFieldId;
    private FormField? selectedField;

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        isLoading = true;
        form = await ApiService.GetFormAsync(FormId);
        availableLists = await ApiService.GetListsAsync();
        isLoading = false;
    }

    // Navigation
    private void GoBack()
    {
        Navigation.NavigateTo("/forms");
    }

    // Field Selection
    private void SelectField(FormField field)
    {
        selectedFieldId = field.Id;
        selectedField = field;
    }

    private void DeselectField()
    {
        selectedFieldId = null;
        selectedField = null;
    }

    // Add Field
    private void OpenAddFieldModal()
    {
        newField = new FormField
        {
            FormId = FormId,
            FieldType = FormFieldType.Text,
            ColumnSize = 12,
            Order = (form?.Fields.Any() == true ? form.Fields.Max(f => f.Order) + 1 : 0)
        };
        showAddFieldModal = true;
    }

    private void CloseAddFieldModal()
    {
        showAddFieldModal = false;
        newField = null;
    }

    private async Task AddField()
    {
        if (form == null || newField == null || string.IsNullOrWhiteSpace(newField.Label)) return;

        var created = await ApiService.AddFieldToFormAsync(FormId, newField);
        if (created != null)
        {
            form.Fields.Add(created);
            CloseAddFieldModal();
        }
    }

    // Save Field
    private async Task SaveSelectedField()
    {
        if (form == null || selectedField == null) return;

        await ApiService.UpdateFieldInFormAsync(FormId, selectedField.Id, selectedField);
        // Reload to get updated data
        await LoadData();
        
        // Try to reselect the same field
        if (selectedFieldId.HasValue && form != null)
        {
            selectedField = form.Fields.FirstOrDefault(f => f.Id == selectedFieldId);
        }
    }

    // Delete Field
    private async Task DeleteSelectedField()
    {
        if (form == null || selectedField == null) return;

        var success = await ApiService.RemoveFieldFromFormAsync(FormId, selectedField.Id);
        if (success)
        {
            DeselectField();
            await LoadData();
        }
    }

    // Linked List Changed
    private void OnLinkedListChanged(ChangeEventArgs e)
    {
        if (selectedField == null) return;

        var value = e.Value?.ToString();
        if (string.IsNullOrEmpty(value))
        {
            selectedField.LinkedListId = null;
            selectedField.LinkedFieldId = null;
            selectedField.SelectValueField = null;
            selectedField.SelectDisplayField = null;
            selectedField.SelectSourceFilterField = null;
            selectedField.SelectSourceFilterValue = null;
            selectedField.CascadeFilterField = null;
        }
        else if (Guid.TryParse(value, out var listId))
        {
            selectedField.LinkedListId = listId;
            selectedField.LinkedFieldId = null;
            selectedField.SelectValueField = null;
            selectedField.SelectDisplayField = null;
            selectedField.SelectSourceFilterField = null;
            selectedField.SelectSourceFilterValue = null;
            selectedField.CascadeFilterField = null;
        }
    }

    // Select Source List Changed (independent from persistence list)
    private void OnSelectSourceListChanged(ChangeEventArgs e)
    {
        if (selectedField == null) return;

        var value = e.Value?.ToString();
        if (string.IsNullOrEmpty(value))
        {
            selectedField.SelectSourceListId = null;
        }
        else if (Guid.TryParse(value, out var listId))
        {
            selectedField.SelectSourceListId = listId;
        }

        // Reset dependent fields when source list changes
        selectedField.SelectValueField = null;
        selectedField.SelectDisplayField = null;
        selectedField.SelectSourceFilterField = null;
        selectedField.SelectSourceFilterValue = null;
        selectedField.CascadeFilterField = null;
    }
}
