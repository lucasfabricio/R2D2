using Microsoft.AspNetCore.Components;
using R2D2.Blazor.Models;
using R2D2.Blazor.Services;
using BlazorBootstrap;

namespace R2D2.Blazor.Pages;

public partial class Data
{
    [Parameter] public Guid ListId { get; set; }

    [Inject]
    private ApiService ApiService { get; set; } = default!;
    
    private List<DynamicList> lists = new();
    private List<TableData> tableData = new();
    private DynamicList? selectedList;
    private Guid selectedListId;
    private bool isLoadingData;
    private Modal dataModal = default!;
    private bool isEditing;
    private Guid editingRecordId;
    private Dictionary<string, object?> formValues = new();
    private Dictionary<Guid, int> dataCountCache = new();
    private Dictionary<Guid, List<TableData>> relatedListData = new();
    private string? errorMessage;

    protected override async Task OnInitializedAsync()
    {
        lists = await ApiService.GetListsAsync();
        if (ListId != Guid.Empty)
            await SelectList(ListId);
    }

    private int GetListDataCount(Guid listId) => dataCountCache.TryGetValue(listId, out var count) ? count : 0;

    private async Task SelectList(Guid listId)
    {
        selectedListId = listId;
        selectedList = lists.FirstOrDefault(l => l.Id == listId);
        isLoadingData = true;
        tableData = await ApiService.GetTableDataAsync(listId);
        dataCountCache[listId] = tableData.Count;
        
        // Load related data for relationship fields
        if (selectedList != null)
        {
            foreach (var field in selectedList.Fields)
            {
                if (field.FieldType == FieldType.Relationship && field.RelatedListId.HasValue)
                {
                    if (!relatedListData.ContainsKey(field.RelatedListId.Value))
                    {
                        relatedListData[field.RelatedListId.Value] = await ApiService.GetTableDataAsync(field.RelatedListId.Value);
                    }
                }
            }
        }
        
        isLoadingData = false;
    }

    private string GetFieldValue(TableData row, string fieldName)
    {
        switch (fieldName)
        {
            case "sys_id": return row.Id.ToString();
            case "sys_created_at": return row.CreatedAt.ToString("g");
            case "sys_updated_at": return row.UpdatedAt.ToString("g");
            default:
                if (row.Values.TryGetValue(fieldName, out var value))
                {
                    var field = selectedList?.Fields.FirstOrDefault(f => f.TechnicalName == fieldName);
                    
                    if (field?.FieldType == FieldType.Relationship && field.RelatedListId.HasValue && value != null)
                    {
                        if (Guid.TryParse(value.ToString(), out var relatedId))
                        {
                            if (relatedListData.TryGetValue(field.RelatedListId.Value, out var data))
                            {
                                var relatedRecord = data.FirstOrDefault(r => r.Id == relatedId);
                                if (relatedRecord != null) return GetRecordLabel(relatedRecord, field);
                            }
                        }
                    }
                    return value?.ToString() ?? "-";
                }
                return "-";
        }
    }

    private string GetFormValue(string fieldName)
    {
        if (formValues.TryGetValue(fieldName, out var value))
            return value?.ToString() ?? "";
        return "";
    }

    private void SetFormValue(string fieldName, string? value)
    {
        formValues[fieldName] = value;
    }

    private List<TableData> GetFilteredRecords(DynamicField field)
    {
        if (!field.RelatedListId.HasValue || !relatedListData.TryGetValue(field.RelatedListId.Value, out var data))
            return new List<TableData>();

        return data;
    }

    private async Task ShowCreateModal()
    {
        formValues = new Dictionary<string, object?>();
        isEditing = false;
        errorMessage = null;
        await dataModal.ShowAsync();
    }

    private async Task EditRecord(TableData row)
    {
        formValues = new Dictionary<string, object?>(row.Values);
        editingRecordId = row.Id;
        isEditing = true;
        errorMessage = null;
        await dataModal.ShowAsync();
    }

    private async Task CloseModal()
    {
        await dataModal.HideAsync();
        formValues = new();
    }

    private async Task SaveRecord()
    {
        try
        {
            errorMessage = null;
            if (isEditing)
                await ApiService.UpdateTableDataAsync(editingRecordId, formValues);
            else
                await ApiService.CreateTableDataAsync(selectedListId, formValues);
            
            // Invalidate local caches for this list so dependent lists load fresh data
            relatedListData.Remove(selectedListId);
            dataCountCache.Remove(selectedListId);

            await CloseModal();
            await SelectList(selectedListId);
        }
        catch (Exception ex) 
        { 
            errorMessage = ex.Message;
            StateHasChanged();
        }
    }

    private async Task DeleteRecord(Guid id)
    {
        try
        {
            await ApiService.DeleteTableDataAsync(id);
            
            // Invalidate local caches for this list so dependent lists load fresh data
            relatedListData.Remove(selectedListId);
            dataCountCache.Remove(selectedListId);

            await SelectList(selectedListId);
        }
        catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
    }

    private string GetRecordLabel(TableData record, DynamicField? field = null)
    {
        // Use RelatedFieldTechnicalName if available
        if (field != null && !string.IsNullOrEmpty(field.RelatedFieldTechnicalName))
        {
            if (record.Values.TryGetValue(field.RelatedFieldTechnicalName, out var val))
                return val?.ToString() ?? record.Id.ToString()[..8];
        }

        var nameField = record.Values.Keys.FirstOrDefault(k => 
            k.ToLower().Contains("name") || k.ToLower().Contains("nome") || 
            k.ToLower().Contains("label") || k.ToLower().Contains("titulo") || 
            k.ToLower().Contains("title"));
            
        if (nameField != null && record.Values[nameField] != null)
            return record.Values[nameField]?.ToString() ?? record.Id.ToString()[..8];
            
        var firstField = record.Values.Keys.FirstOrDefault(k => !k.ToLower().Contains("id"));
        if (firstField != null && record.Values[firstField] != null)
            return record.Values[firstField]?.ToString() ?? record.Id.ToString()[..8];
            
        return record.Id.ToString()[..8];
    }
}
