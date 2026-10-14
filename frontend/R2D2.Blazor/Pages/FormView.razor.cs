using Microsoft.AspNetCore.Components;
using R2D2.Blazor.Models;
using R2D2.Blazor.Services;

namespace R2D2.Blazor.Pages;

public partial class FormView
{
    [Parameter] public Guid FormId { get; set; }

    [SupplyParameterFromQuery(Name = "recordId")]
    public string? RecordIdParam { get; set; }

    private Guid? RecordId => Guid.TryParse(RecordIdParam, out var id) ? id : null;

    [Inject]
    private ApiService ApiService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private DynamicForm? form;
    private Dictionary<Guid, string?> fieldValues = new();
    private Dictionary<Guid, List<KeyValuePair<string, string>>> selectOptions = new();
    private bool isLoading = true;
    private bool isSubmitting = false;
    private bool isSubmitted = false;
    private string? errorMessage;
    private List<string> workflowErrors = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadForm();
    }

    private async Task LoadForm()
    {
        isLoading = true;
        errorMessage = null;
        try
        {
            form = await ApiService.GetFormAsync(FormId);
            
            if (form != null)
            {
                // Initialize field values and load select options
                foreach (var field in form.Fields.OrderBy(f => f.Order))
                {
                    fieldValues[field.Id] = null;

                    // Load options for Select fields with a source list
                    // Only load root selects (no parent) on init — children load when parent is selected
                    if (field.FieldType == FormFieldType.Select 
                        && !field.ParentFormFieldId.HasValue
                        && (field.SelectSourceListId.HasValue || field.LinkedListId.HasValue))
                    {
                        await LoadSelectOptions(field);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Erro ao carregar formulário: {ex.Message}";
        }
        isLoading = false;

        // If this is an Update form with a RecordId, load the record data
        if (form != null && form.FormType == 1 && RecordId.HasValue)
        {
            await LoadRecordData(RecordId.Value);
        }
    }

    private async Task LoadRecordData(Guid recordId)
    {
        try
        {
            var record = await ApiService.GetTableDataByIdAsync(recordId);
            if (record != null && form != null)
            {
                foreach (var field in form.Fields)
                {
                    string? fieldKey = field.LinkedFieldName ?? field.Label;
                    if (fieldKey != null && record.Values.TryGetValue(fieldKey, out var val))
                    {
                        fieldValues[field.Id] = val?.ToString();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading record data: {ex.Message}");
        }
    }

    private async Task LoadSelectOptions(FormField field, string? parentFilterValue = null)
    {
        // Use SelectSourceListId (data source) if set, otherwise fall back to LinkedListId
        var sourceListId = field.SelectSourceListId ?? field.LinkedListId;
        if (!sourceListId.HasValue) return;

        var records = await ApiService.GetTableDataAsync(sourceListId.Value);

        // Apply static filter if configured
        if (!string.IsNullOrEmpty(field.SelectSourceFilterField) && 
            !string.IsNullOrEmpty(field.SelectSourceFilterValue))
        {
            records = records.Where(r =>
                r.Values.TryGetValue(field.SelectSourceFilterField, out var v) &&
                v?.ToString()?.ToLower() == field.SelectSourceFilterValue.ToLower()
            ).ToList();
        }

        var options = new List<KeyValuePair<string, string>>();

        // Apply cascade filter if configured
        if (!string.IsNullOrEmpty(field.CascadeFilterField) && 
            field.ParentFormFieldId.HasValue && 
            !string.IsNullOrEmpty(parentFilterValue))
        {
            records = records.Where(r =>
                r.Values.TryGetValue(field.CascadeFilterField, out var v) &&
                v?.ToString()?.ToLower() == parentFilterValue.ToLower()
            ).ToList();
        }

        foreach (var record in records)
        {
            // Determine key (value) based on SelectValueField
            string key;
            if (!string.IsNullOrEmpty(field.SelectValueField))
            {
                if (field.SelectValueField.ToLower() == "sys_id" || field.SelectValueField.ToLower() == "id")
                    key = record.Id.ToString();
                else if (record.Values.TryGetValue(field.SelectValueField, out var kv))
                    key = kv?.ToString() ?? record.Id.ToString();
                else
                    key = record.Id.ToString();
            }
            else
            {
                key = record.Id.ToString(); // Default: record ID
            }

            // Determine display text based on SelectDisplayField
            string displayValue;
            if (!string.IsNullOrEmpty(field.SelectDisplayField))
            {
                if (field.SelectDisplayField.ToLower() == "sys_id" || field.SelectDisplayField.ToLower() == "id")
                    displayValue = record.Id.ToString();
                else if (record.Values.TryGetValue(field.SelectDisplayField, out var dv))
                    displayValue = dv?.ToString() ?? key;
                else
                    displayValue = key;
            }
            else
            {
                // Auto-detect: look for name-like fields
                displayValue = key.Length > 8 ? key.Substring(0, 8) : key;
                var nameField = record.Values.Keys.FirstOrDefault(k =>
                    k.ToLower().Contains("name") ||
                    k.ToLower().Contains("nome") ||
                    k.ToLower().Contains("label") ||
                    k.ToLower().Contains("titulo") ||
                    k.ToLower().Contains("title"));

                if (nameField != null && record.Values[nameField] != null)
                    displayValue = record.Values[nameField]?.ToString() ?? displayValue;
                else
                {
                    var firstField = record.Values.Keys.FirstOrDefault(k => !k.ToLower().Contains("id"));
                    if (firstField != null && record.Values[firstField] != null)
                        displayValue = record.Values[firstField]?.ToString() ?? displayValue;
                }
            }

            options.Add(new KeyValuePair<string, string>(key, displayValue));
        }

        selectOptions[field.Id] = options;
    }

    private string? GetFieldValue(FormField field)
    {
        return fieldValues.TryGetValue(field.Id, out var val) ? val : null;
    }

    private string GetDateValue(FormField field)
    {
        if (fieldValues.TryGetValue(field.Id, out var val) && !string.IsNullOrEmpty(val))
        {
            if (DateTime.TryParse(val, out var dt)) return dt.ToString("yyyy-MM-dd");
        }
        return "";
    }

    private void SetFieldValue(FormField field, object? value)
    {
        fieldValues[field.Id] = value?.ToString();
    }

    private async Task HandleSelectChange(FormField field, object? value)
    {
        fieldValues[field.Id] = value?.ToString();

        // Recursively reset all transitive dependents
        if (form != null)
        {
            await ResetDependents(field.Id, value?.ToString());
        }

        StateHasChanged();
    }

    private async Task ResetDependents(Guid parentFieldId, string? parentValue)
    {
        if (form == null) return;

        foreach (var dependentField in form.Fields.Where(f => f.ParentFormFieldId == parentFieldId))
        {
            fieldValues[dependentField.Id] = null; // Reset value
            selectOptions.Remove(dependentField.Id); // Clear old options

            // Only reload options if parent has a selected value
            // This prevents loading Cidade when only País was selected (Estado is still empty)
            if (!string.IsNullOrEmpty(parentValue) 
                && (dependentField.SelectSourceListId.HasValue || dependentField.LinkedListId.HasValue))
            {
                await LoadSelectOptions(dependentField, parentValue);
            }

            // Recursively reset this field's children too (with null since this field was reset)
            await ResetDependents(dependentField.Id, null);
        }
    }

    private async Task HandleSubmit()
    {
        if (form == null) return;
        
        isSubmitting = true;
        errorMessage = null;
        workflowErrors = new();
        
        try
        {
            // Build values dictionary from ALL form fields for workflow context
            var saveValues = new Dictionary<string, object?>();
            Guid? targetListId = null;

            foreach (var field in form.Fields)
            {
                // Determine the key for this field's value
                string? fieldKey = null;

                if (field.LinkedListId.HasValue && field.LinkedFieldId.HasValue)
                {
                    targetListId ??= field.LinkedListId;
                    fieldKey = field.LinkedFieldName ?? field.Label;
                }
                else
                {
                    // Include non-linked fields using the Label as key
                    // This allows InsertData rules to find values by column name matching
                    fieldKey = field.Label;
                }

                if (fieldKey != null && fieldValues.TryGetValue(field.Id, out var val) && val != null)
                {
                    saveValues[fieldKey] = val;
                }
            }

            // Determine submission mode based on FormType
            bool isUpdateForm = form.FormType == 1;

            // 1. Execute Workflow FIRST (for validation and possible InsertData rules)
            bool workflowHandledInsert = false;
            if (form.WorkflowId != null)
            {
                var result = await ApiService.ExecuteWorkflowAsync(form.WorkflowId.Value, saveValues);
                if (result != null && !result.Success)
                {
                    errorMessage = "Validação falhou. Por favor corrija os erros e tente novamente.";
                    workflowErrors = result.Errors;
                    isSubmitting = false;
                    return;
                }

                if (result != null && result.Success)
                {
                    workflowHandledInsert = true;
                }
            }

            // 2. Save or Update
            if (isUpdateForm && RecordId.HasValue)
            {
                // Update existing record
                await ApiService.UpdateTableDataAsync(RecordId.Value, saveValues);
            }
            else if (!workflowHandledInsert && targetListId.HasValue && saveValues.Any())
            {
                await ApiService.CreateTableDataAsync(targetListId.Value, saveValues);
            }

            isSubmitted = true;
        }
        catch (Exception ex)
        {
            errorMessage = $"Falha ao enviar: {ex.Message}";
        }
        isSubmitting = false;
    }

    private async Task ResetForm()
    {
        isSubmitted = false;
        fieldValues = new();
        await LoadForm();
    }
}
