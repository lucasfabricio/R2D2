using R2D2.Blazor.Models;
using R2D2.Blazor.Services;
using Microsoft.AspNetCore.Components;
using BlazorBootstrap;

namespace R2D2.Blazor.Pages;

public partial class Lists : ComponentBase
{
    private List<DynamicList> lists = new();
    private bool isLoading = true;
    private Modal listModal = default!;
    private DynamicList? editingList = new();
    private Guid? editingFieldId;
    private DynamicField? newFieldBuffer;

    protected override async Task OnInitializedAsync()
    {
        await LoadLists();
    }

    private async Task LoadLists()
    {
        isLoading = true;
        try
        {
            lists = await ApiService.GetListsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        isLoading = false;
    }

    private async Task OpenCreateModal()
    {
        editingList = new DynamicList();
        editingFieldId = null;
        await listModal.ShowAsync();
    }

    private async Task OpenEditModal(DynamicList list)
    {
        editingList = new DynamicList
        {
            Id = list.Id,
            Name = list.Name,
            DisplayName = list.DisplayName,
            Description = list.Description,
            CreatedAt = list.CreatedAt,
            Fields = list.Fields.Select(f => new DynamicField
            {
                Id = f.Id,
                ListId = f.ListId,
                TechnicalName = f.TechnicalName,
                Label = f.Label,
                FieldType = f.FieldType,
                IsRequired = f.IsRequired,
                MaxLength = f.MaxLength,
                RelatedListId = f.RelatedListId,
                RelatedFieldTechnicalName = f.RelatedFieldTechnicalName,
                SelectOptions = f.SelectOptions,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            }).ToList()
        };
        editingFieldId = null;
        await listModal.ShowAsync();
    }

    private async Task CloseModals()
    {
        await listModal.HideAsync();
        editingList = null;
        editingFieldId = null;
        newFieldBuffer = null;
    }

    private async Task SaveList()
    {
        if (editingList == null) return;

        if (editingList.Id == Guid.Empty)
        {
            var created = await ApiService.CreateListAsync(editingList);
            if (created != null)
            {
                editingList = new DynamicList
                {
                    Id = created.Id,
                    Name = created.Name,
                    DisplayName = created.DisplayName,
                    Description = created.Description,
                    CreatedAt = created.CreatedAt,
                    Fields = created.Fields.ToList()
                };
                await LoadLists();
                // Keep modal open so user can add fields
                return;
            }
        }
        else
        {
            await ApiService.UpdateListAsync(editingList.Id, editingList);
        }

        await LoadLists();
        await CloseModals();
    }

    private async Task DeleteList(DynamicList list)
    {
        await ApiService.DeleteListAsync(list.Id);
        await LoadLists();
    }

    // Field Operations
    private void AddNewField()
    {
        if (editingList == null) return;

        newFieldBuffer = new DynamicField
        {
            Id = Guid.NewGuid(), // Temporary ID
            ListId = editingList.Id,
            Label = "",
            TechnicalName = "",
            FieldType = FieldType.Text
        };
        editingList.Fields.Add(newFieldBuffer);
        editingFieldId = newFieldBuffer.Id;
    }

    private void StartEditField(DynamicField field)
    {
        editingFieldId = field.Id;
    }

    private void CancelFieldEdit()
    {
        if (newFieldBuffer != null && editingList != null)
        {
            editingList.Fields.Remove(newFieldBuffer);
            newFieldBuffer = null;
        }
        editingFieldId = null;
    }

    private async Task SaveField(DynamicField field)
    {
        if (editingList == null) return;

        if (newFieldBuffer != null && field.Id == newFieldBuffer.Id)
        {
            // New field - add to backend
            var success = await ApiService.AddFieldToListAsync(editingList.Id, field);
            if (success)
            {
                newFieldBuffer = null;
                // Reload to get the real ID
                var updatedList = await ApiService.GetListAsync(editingList.Id);
                if (updatedList != null)
                {
                    editingList.Fields = updatedList.Fields.ToList();
                    await LoadLists();
                }
            }
        }
        else
        {
            // Existing field - update
            await ApiService.UpdateFieldInListAsync(editingList.Id, field.Id, field);
            await LoadLists();
        }

        editingFieldId = null;
    }

    private async Task DeleteField(DynamicField field)
    {
        if (editingList == null) return;

        if (newFieldBuffer != null && field.Id == newFieldBuffer.Id)
        {
            editingList.Fields.Remove(field);
            newFieldBuffer = null;
            editingFieldId = null;
            return;
        }

        var success = await ApiService.RemoveFieldFromListAsync(editingList.Id, field.Id);
        if (success)
        {
            editingList.Fields.Remove(field);
            editingFieldId = null;
            await LoadLists();
        }
    }
}
