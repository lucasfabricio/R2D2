using R2D2.Blazor.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace R2D2.Blazor.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly Blazored.LocalStorage.ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;
    private string? _authToken;

    public ApiService(HttpClient httpClient, Blazored.LocalStorage.ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _authToken = await _localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(_authToken))
            {
                SetAuthToken(_authToken);
            }
        }
        catch { }
    }

    private void SetAuthToken(string token)
    {
        _authToken = token;
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public async Task ClearAuthTokenAsync()
    {
        _authToken = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
        await _localStorage.RemoveItemAsync("authToken");
        ((CustomAuthenticationStateProvider)_authStateProvider).NotifyUserLogout();
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(_authToken);

    // Auth
    public async Task<AuthResponse?> LoginAsync(string username, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/login", new { Username = username, Password = password });
            Console.WriteLine($"[Auth] Login response status: {response.StatusCode}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (result != null)
                {
                    Console.WriteLine($"[Auth] Token received. Length: {result.Token?.Length}");
                    SetAuthToken(result.Token);
                    await _localStorage.SetItemAsync("authToken", result.Token);
                    Console.WriteLine("[Auth] Calling NotifyUserAuthentication...");
                    ((CustomAuthenticationStateProvider)_authStateProvider).NotifyUserAuthentication(result.Token);
                    Console.WriteLine("[Auth] NotifyUserAuthentication completed.");
                }
                else
                {
                    Console.WriteLine("[Auth] Result is null.");
                }
                return result;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[Auth] Login failed. Status: {response.StatusCode}, Error: {error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Auth] Exception in LoginAsync: {ex}");
        }
        return null;
    }

    public async Task<bool> RegisterAsync(string username, string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("auth/register", 
            new { Username = username, Email = email, Password = password });
        return response.IsSuccessStatusCode;
    }

    // Dynamic Lists
    public async Task<List<DynamicList>> GetListsAsync()
    {
        try { return await _httpClient.GetFromJsonAsync<List<DynamicList>>("dynamiclists") ?? new(); }
        catch { return new(); }
    }

    public async Task<DynamicList?> GetListAsync(Guid id)
    {
        try { return await _httpClient.GetFromJsonAsync<DynamicList>($"dynamiclists/{id}"); }
        catch { return null; }
    }

    public async Task<List<DynamicField>> GetListFieldsAsync(Guid listId)
    {
        try
        {
            var json = await _httpClient.GetFromJsonAsync<System.Text.Json.JsonElement>($"dynamiclists/{listId}");
            if (json.TryGetProperty("fields", out var fields))
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<DynamicField>>(fields.GetRawText(),
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
        }
        catch { }
        return new();
    }

    public async Task<DynamicList?> CreateListAsync(DynamicList list)
    {
        var response = await _httpClient.PostAsJsonAsync("dynamiclists", new { list.Name, list.DisplayName, list.Description });
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<DynamicList>();
        return null;
    }

    public async Task<bool> UpdateListAsync(Guid id, DynamicList list)
    {
        var response = await _httpClient.PutAsJsonAsync($"dynamiclists/{id}", new { list.Name, list.DisplayName, list.Description });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteListAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"dynamiclists/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> AddFieldToListAsync(Guid listId, DynamicField field)
    {
        var response = await _httpClient.PostAsJsonAsync($"dynamiclists/{listId}/fields", 
            new { 
                field.TechnicalName, 
                field.Label, 
                field.FieldType, 
                field.IsRequired,
                field.MaxLength,
                field.RelatedListId,
                field.RelatedFieldTechnicalName
            });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveFieldFromListAsync(Guid listId, Guid fieldId)
    {
        var response = await _httpClient.DeleteAsync($"dynamiclists/{listId}/fields/{fieldId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateFieldInListAsync(Guid listId, Guid fieldId, DynamicField field)
    {
        var response = await _httpClient.PutAsJsonAsync($"dynamiclists/{listId}/fields/{fieldId}", 
            new { 
                field.TechnicalName, 
                field.Label, 
                field.FieldType, 
                field.IsRequired,
                field.MaxLength,
                field.RelatedListId,
                field.RelatedFieldTechnicalName
            });
        return response.IsSuccessStatusCode;
    }

    // Dynamic Forms
    public async Task<List<DynamicForm>> GetFormsAsync()
    {
        try { return await _httpClient.GetFromJsonAsync<List<DynamicForm>>("dynamicforms") ?? new(); }
        catch { return new(); }
    }

    public async Task<DynamicForm?> GetFormAsync(Guid id)
    {
        try { return await _httpClient.GetFromJsonAsync<DynamicForm>($"dynamicforms/{id}"); }
        catch { return null; }
    }

    public async Task<DynamicForm?> CreateFormAsync(DynamicForm form)
    {
        try
        {
            var dto = new { form.Name, form.Description, form.FormType, form.WorkflowId };
            var response = await _httpClient.PostAsJsonAsync("dynamicforms", dto);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<DynamicForm>();
            
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error creating form: {error}");
        }
        catch (Exception ex) { Console.WriteLine($"Exception creating form: {ex.Message}"); }
        return null;
    }

    public async Task<bool> UpdateFormAsync(Guid id, DynamicForm form)
    {
        try
        {
            var dto = new { form.Name, form.Description, form.FormType, form.WorkflowId };
            var response = await _httpClient.PutAsJsonAsync($"dynamicforms/{id}", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) 
        { 
            Console.WriteLine($"Exception updating form {id}: {ex.Message}");
            return false; 
        }
    }

    public async Task<bool> DeleteFormAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"dynamicforms/{id}");
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<FormField?> AddFieldToFormAsync(Guid formId, FormField field)
    {
        var dto = new
        {
            field.Label,
            field.FieldType,
            field.Order,
            field.ColumnSize,
            field.IsReadOnly,
            field.LinkedListId,
            field.LinkedFieldId,
            field.ParentFormFieldId,
            field.CascadeFilterField,
            field.SelectSourceListId,
            field.SelectValueField,
            field.SelectDisplayField,
            field.SelectSourceFilterField,
            field.SelectSourceFilterValue
        };
        var response = await _httpClient.PostAsJsonAsync($"dynamicforms/{formId}/fields", dto);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<FormField>();
        return null;
    }

    public async Task<bool> UpdateFieldInFormAsync(Guid formId, Guid formFieldId, FormField field)
    {
        var dto = new
        {
            field.Label,
            field.FieldType,
            field.Order,
            field.ColumnSize,
            field.IsReadOnly,
            field.LinkedListId,
            field.LinkedFieldId,
            field.ParentFormFieldId,
            field.CascadeFilterField,
            field.SelectSourceListId,
            field.SelectValueField,
            field.SelectDisplayField,
            field.SelectSourceFilterField,
            field.SelectSourceFilterValue
        };
        var response = await _httpClient.PutAsJsonAsync($"dynamicforms/{formId}/fields/{formFieldId}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveFieldFromFormAsync(Guid formId, Guid formFieldId)
    {
        var response = await _httpClient.DeleteAsync($"dynamicforms/{formId}/fields/{formFieldId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateFieldOrderInFormAsync(Guid formId, List<Guid> fieldOrder)
    {
        var response = await _httpClient.PutAsJsonAsync($"dynamicforms/{formId}/fields/order", fieldOrder);
        return response.IsSuccessStatusCode;
    }

    // Business Rules
    public async Task<List<BusinessRule>> GetRulesAsync()
    {
        try { return await _httpClient.GetFromJsonAsync<List<BusinessRule>>("businessrules") ?? new(); }
        catch { return new(); }
    }

    public async Task<BusinessRule?> GetRuleAsync(Guid id)
    {
        try { return await _httpClient.GetFromJsonAsync<BusinessRule>($"businessrules/{id}"); }
        catch { return null; }
    }

    public async Task<BusinessRule?> CreateRuleAsync(BusinessRule rule)
    {
        var response = await _httpClient.PostAsJsonAsync("businessrules", rule);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<BusinessRule>();
        return null;
    }

    public async Task<bool> UpdateRuleAsync(Guid id, BusinessRule rule)
    {
        var response = await _httpClient.PutAsJsonAsync($"businessrules/{id}", rule);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteRuleAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"businessrules/{id}");
        return response.IsSuccessStatusCode;
    }

    // Workflows
    public async Task<List<Workflow>> GetWorkflowsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Workflow>>("workflows") ?? new();
    }

    public async Task<Workflow?> GetWorkflowAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<Workflow>($"workflows/{id}");
    }

    public async Task<Workflow?> CreateWorkflowAsync(Workflow workflow)
    {
        var response = await _httpClient.PostAsJsonAsync("workflows", workflow);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<Workflow>();
        return null;
    }

    public async Task<bool> UpdateWorkflowAsync(Guid id, Workflow workflow)
    {
        var response = await _httpClient.PutAsJsonAsync($"workflows/{id}", workflow);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteWorkflowAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"workflows/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> AddStepToWorkflowAsync(Guid workflowId, Guid ruleId, int order = 0)
    {
        var response = await _httpClient.PostAsJsonAsync($"workflows/{workflowId}/steps", 
            new { RuleId = ruleId, Order = order, PositionX = 0, PositionY = 0 });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveStepFromWorkflowAsync(Guid workflowId, Guid stepId)
    {
        var response = await _httpClient.DeleteAsync($"workflows/{workflowId}/steps/{stepId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<WorkflowExecutionResult?> ExecuteWorkflowAsync(Guid id, Dictionary<string, object?> context)
    {
        var response = await _httpClient.PostAsJsonAsync($"workflows/{id}/execute", context);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<WorkflowExecutionResult>();
        return null;
    }

    // Table Data
    public async Task<List<TableData>> GetTableDataAsync(Guid listId)
    {
        try { return await _httpClient.GetFromJsonAsync<List<TableData>>($"tabledata/list/{listId}") ?? new(); }
        catch { return new(); }
    }

    public async Task<TableData?> GetTableDataByIdAsync(Guid id)
    {
        try { return await _httpClient.GetFromJsonAsync<TableData>($"tabledata/{id}"); }
        catch { return null; }
    }

    public async Task<TableData?> CreateTableDataAsync(Guid listId, Dictionary<string, object?> values)
    {
        var response = await _httpClient.PostAsJsonAsync("tabledata", new { ListId = listId, Values = values });
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<TableData>();
        
        var error = await response.Content.ReadAsStringAsync();
        throw new Exception($"Failed to create record: {response.StatusCode}. {error}");
    }

    public async Task<bool> UpdateTableDataAsync(Guid id, Dictionary<string, object?> values)
    {
        var response = await _httpClient.PutAsJsonAsync($"tabledata/{id}", new { Values = values });
        if (response.IsSuccessStatusCode)
            return true;
        
        var error = await response.Content.ReadAsStringAsync();
        throw new Exception($"Failed to update record: {response.StatusCode}. {error}");
    }

    public async Task<bool> DeleteTableDataAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"tabledata/{id}");
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    // List Views
    public async Task<List<ListView>> GetListViewsAsync()
    {
        try { return await _httpClient.GetFromJsonAsync<List<ListView>>("listviews") ?? new(); }
        catch { return new(); }
    }

    public async Task<ListView?> GetListViewAsync(Guid id)
    {
        try { return await _httpClient.GetFromJsonAsync<ListView>($"listviews/{id}"); }
        catch { return null; }
    }

    public async Task<ListView?> CreateListViewAsync(ListView view)
    {
        var dto = new { view.Name, view.DisplayName, view.ListId, view.CreateFormId, view.UpdateFormId };
        var response = await _httpClient.PostAsJsonAsync("listviews", dto);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<ListView>();
        return null;
    }

    public async Task<bool> UpdateListViewAsync(Guid id, ListView view)
    {
        var dto = new { view.Name, view.DisplayName, view.ListId, view.CreateFormId, view.UpdateFormId };
        var response = await _httpClient.PutAsJsonAsync($"listviews/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteListViewAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"listviews/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ReplaceListViewColumnsAsync(Guid listViewId, List<object> columns)
    {
        var response = await _httpClient.PutAsJsonAsync($"listviews/{listViewId}/columns", columns);
        return response.IsSuccessStatusCode;
    }
}
