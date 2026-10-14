using Microsoft.AspNetCore.Components;
using R2D2.Blazor.Models;
using R2D2.Blazor.Services;
using BlazorBootstrap;
using System.Text.Json;

namespace R2D2.Blazor.Pages;

public partial class Rules
{
    [Inject]
    private ApiService ApiService { get; set; } = default!;

    private List<BusinessRule> rules = new();
    private bool isLoading = true;
    private Modal ruleModal = default!;
    private bool isEditing = false;
    private BusinessRule currentRule = new();

    // InsertData config state
    private List<DynamicList> availableLists = new();
    private List<DynamicField> availableFields = new();
    private string selectedListId = "";
    private List<string> selectedColumns = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadRules();
        await LoadLists();
    }

    private async Task LoadRules()
    {
        isLoading = true;
        try { rules = await ApiService.GetRulesAsync(); }
        catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
        isLoading = false;
    }

    private async Task LoadLists()
    {
        try { availableLists = await ApiService.GetListsAsync(); }
        catch (Exception ex) { Console.WriteLine($"Error loading lists: {ex.Message}"); }
    }

    private string GetRuleIcon(RuleType type) => type switch
    {
        RuleType.Validation => "✓",
        RuleType.InsertData => "➕",
        RuleType.UpdateData => "🔄",
        RuleType.Calculate => "🔢",
        RuleType.SendEmail => "📧",
        RuleType.Condition => "❓",
        _ => "⚙️"
    };

    private async Task ShowCreateModal()
    {
        currentRule = new BusinessRule { ConfigurationJson = "{}" };
        isEditing = false;
        ResetInsertDataState();
        await ruleModal.ShowAsync();
    }

    private async Task EditRule(BusinessRule rule)
    {
        currentRule = new BusinessRule
        {
            Id = rule.Id, Name = rule.Name, Description = rule.Description,
            Type = rule.Type, ConfigurationJson = rule.ConfigurationJson,
            IsActive = rule.IsActive
        };
        isEditing = true;

        ResetInsertDataState();

        if (rule.Type == RuleType.InsertData)
        {
            await LoadInsertDataConfigFromJson(rule.ConfigurationJson);
        }

        await ruleModal.ShowAsync();
    }

    private async Task CloseModal() { await ruleModal.HideAsync(); currentRule = new BusinessRule(); ResetInsertDataState(); }

    private async Task SaveRule()
    {
        try
        {
            if (currentRule.Type == RuleType.InsertData)
            {
                BuildInsertDataConfigJson();
            }

            if (isEditing) await ApiService.UpdateRuleAsync(currentRule.Id, currentRule);
            else await ApiService.CreateRuleAsync(currentRule);
            await CloseModal();
            await LoadRules();
        }
        catch (Exception ex) { Console.WriteLine($"Error saving rule: {ex.Message}"); }
    }

    private async Task ToggleRule(BusinessRule rule)
    {
        try
        {
            rule.IsActive = !rule.IsActive;
            await ApiService.UpdateRuleAsync(rule.Id, rule);
            await LoadRules();
        }
        catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
    }

    private async Task DeleteRule(Guid id)
    {
        try { await ApiService.DeleteRuleAsync(id); await LoadRules(); }
        catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
    }

    // === InsertData helpers ===

    private async Task OnRuleTypeChanged(ChangeEventArgs e)
    {
        if (Enum.TryParse<RuleType>(e.Value?.ToString(), out var newType))
        {
            currentRule.Type = newType;
            if (newType == RuleType.InsertData)
            {
                await LoadLists();
            }
        }
    }

    private async Task OnTargetListChanged(ChangeEventArgs e)
    {
        var value = e.Value?.ToString() ?? "";
        selectedListId = value;
        availableFields.Clear();
        selectedColumns.Clear();

        if (Guid.TryParse(value, out var listId))
        {
            try
            {
                var list = await ApiService.GetListAsync(listId);
                if (list != null)
                {
                    availableFields = list.Fields;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading fields: {ex.Message}");
            }
        }
    }

    private void ToggleColumn(string technicalName, bool isChecked)
    {
        if (isChecked && !selectedColumns.Contains(technicalName))
        {
            selectedColumns.Add(technicalName);
        }
        else if (!isChecked)
        {
            selectedColumns.Remove(technicalName);
        }
    }

    private void ResetInsertDataState()
    {
        selectedListId = "";
        availableFields.Clear();
        selectedColumns.Clear();
    }

    private async Task LoadInsertDataConfigFromJson(string json)
    {
        try
        {
            var config = JsonSerializer.Deserialize<InsertDataConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (config == null) return;

            if (config.TargetListId != Guid.Empty)
            {
                selectedListId = config.TargetListId.ToString();

                var list = await ApiService.GetListAsync(config.TargetListId);
                if (list != null)
                {
                    availableFields = list.Fields;
                }

                selectedColumns = config.TargetColumns ?? new();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing InsertData config: {ex.Message}");
        }
    }

    private void BuildInsertDataConfigJson()
    {
        var config = new InsertDataConfig
        {
            TargetListId = Guid.TryParse(selectedListId, out var id) ? id : Guid.Empty,
            TargetColumns = selectedColumns
        };

        currentRule.ConfigurationJson = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    // Local DTO for serialization/deserialization of InsertData config
    private class InsertDataConfig
    {
        public Guid TargetListId { get; set; }
        public List<string> TargetColumns { get; set; } = new();
    }
}
