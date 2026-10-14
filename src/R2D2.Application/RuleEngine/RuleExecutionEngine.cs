using R2D2.Domain.Entities;
using R2D2.Domain.Enums;
using R2D2.Application.RuleEngine.Configs;
using R2D2.Application.Interfaces;
using R2D2.Application.DTOs;
using System.Text.Json;

using JsonOpts = System.Text.Json.JsonSerializerOptions;

namespace R2D2.Application.RuleEngine;

public class RuleExecutionEngine : IRuleExecutionEngine
{
    private readonly ITableDataService _tableDataService;
    private static readonly JsonOpts _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public RuleExecutionEngine(ITableDataService tableDataService)
    {
        _tableDataService = tableDataService;
    }

    public async Task<RuleExecutionResult> ExecuteRuleAsync(BusinessRule rule, RuleExecutionContext context)
    {
        try
        {
            return rule.Type switch
            {
                RuleType.Validation => await ExecuteValidationRule(rule, context),
                RuleType.InsertData => await ExecuteInsertDataRule(rule, context),
                RuleType.UpdateData => await ExecuteUpdateDataRule(rule, context),
                RuleType.Calculate => await ExecuteCalculateRule(rule, context),
                RuleType.SendEmail => await ExecuteSendEmailRule(rule, context),
                RuleType.Condition => await ExecuteConditionRule(rule, context),
                RuleType.Restriction => await ExecuteRestrictionRule(rule, context),
                _ => new RuleExecutionResult { Success = false, Message = "Unknown rule type" }
            };
        }
        catch (Exception ex)
        {
            return new RuleExecutionResult
            {
                Success = false,
                Message = "Rule execution failed",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    private Task<RuleExecutionResult> ExecuteValidationRule(BusinessRule rule, RuleExecutionContext context)
    {
        var config = JsonSerializer.Deserialize<ValidationRuleConfig>(rule.ConfigurationJson, _jsonOptions);
        if (config == null)
            return Task.FromResult(new RuleExecutionResult { Success = false, Message = "Invalid configuration" });

        var value = context.GetVariable(config.FieldName);
        var stringValue = value?.ToString() ?? "";

        bool isValid = true;
        var errors = new List<string>();

        if (config.Required && string.IsNullOrEmpty(stringValue))
        {
            isValid = false;
            errors.Add($"{config.FieldName} is required");
        }

        if (!string.IsNullOrEmpty(config.Pattern) && !System.Text.RegularExpressions.Regex.IsMatch(stringValue, config.Pattern))
        {
            isValid = false;
            errors.Add($"{config.FieldName} does not match the required pattern");
        }

        return Task.FromResult(new RuleExecutionResult
        {
            Success = isValid,
            Message = isValid ? "Validation passed" : "Validation failed",
            Errors = errors
        });
    }

    private async Task<RuleExecutionResult> ExecuteInsertDataRule(BusinessRule rule, RuleExecutionContext context)
    {
        var config = JsonSerializer.Deserialize<DataRuleConfig>(rule.ConfigurationJson, _jsonOptions);
        if (config == null)
            return new RuleExecutionResult { Success = false, Message = "Invalid configuration" };

        if (config.TargetListId == Guid.Empty)
            return new RuleExecutionResult { Success = false, Message = "Target list not specified" };

        try
        {
            var values = new Dictionary<string, object?>();

            foreach (var column in config.TargetColumns)
            {
                // Auto-match: try multiple strategies to find the right context variable
                object? value = null;

                // Strategy 1: Exact match by column name
                value = context.GetVariable(column);

                // Strategy 2: Case-insensitive match
                if (value == null)
                {
                    var key = context.Variables.Keys
                        .FirstOrDefault(k => k.Equals(column, StringComparison.OrdinalIgnoreCase));
                    if (key != null) value = context.GetVariable(key);
                }

                // Strategy 3: Column name starts with context key (e.g., column "EstadoId" matches context "Estado")
                if (value == null)
                {
                    var key = context.Variables.Keys
                        .FirstOrDefault(k => column.StartsWith(k, StringComparison.OrdinalIgnoreCase));
                    if (key != null) value = context.GetVariable(key);
                }

                // Strategy 4: Context key starts with column name (e.g., context "NomeCompleto" matches column "Nome")
                if (value == null)
                {
                    var key = context.Variables.Keys
                        .FirstOrDefault(k => k.StartsWith(column, StringComparison.OrdinalIgnoreCase));
                    if (key != null) value = context.GetVariable(key);
                }

                // Strategy 5: If FieldMappings is provided (legacy), use it as explicit override
                if (value == null && config.FieldMappings.TryGetValue(column, out var mappedSource))
                {
                    value = context.GetVariable(mappedSource);
                }

                // Convert JsonElement to primitive values
                value = ExtractJsonValue(value);

                if (value != null)
                    values[column] = value;
            }

            if (values.Count == 0)
                return new RuleExecutionResult { Success = false, Message = "No values resolved from context for the target columns" };

            var dto = new CreateTableDataDto(config.TargetListId, values);
            var result = await _tableDataService.CreateAsync(dto);

            return new RuleExecutionResult
            {
                Success = true,
                Message = $"Data inserted successfully into list {config.TargetListId}",
                Data = new { RecordId = result.Id, ListId = config.TargetListId, InsertedFields = values.Keys.ToList() }
            };
        }
        catch (Exception ex)
        {
            return new RuleExecutionResult
            {
                Success = false,
                Message = "Failed to insert data",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    private Task<RuleExecutionResult> ExecuteUpdateDataRule(BusinessRule rule, RuleExecutionContext context)
    {
        var config = JsonSerializer.Deserialize<DataRuleConfig>(rule.ConfigurationJson, _jsonOptions);
        if (config == null)
            return Task.FromResult(new RuleExecutionResult { Success = false, Message = "Invalid configuration" });

        return Task.FromResult(new RuleExecutionResult
        {
            Success = true,
            Message = $"Data update simulated for list {config.TargetListId}",
            Data = new { ListId = config.TargetListId, FieldMappings = config.FieldMappings }
        });
    }

    private Task<RuleExecutionResult> ExecuteCalculateRule(BusinessRule rule, RuleExecutionContext context)
    {
        var config = JsonSerializer.Deserialize<CalculateRuleConfig>(rule.ConfigurationJson, _jsonOptions);
        if (config == null)
            return Task.FromResult(new RuleExecutionResult { Success = false, Message = "Invalid configuration" });

        try
        {
            // Simple expression evaluation for MVP
            // In production, use a proper expression evaluator
            double result = 0;
            var expression = config.Expression;

            foreach (var variable in context.Variables)
            {
                if (double.TryParse(variable.Value?.ToString(), out var numValue))
                {
                    expression = expression.Replace($"{{{variable.Key}}}", numValue.ToString());
                }
            }

            // Basic eval simulation (in production use a proper evaluator)
            if (double.TryParse(expression, out result))
            {
                context.SetVariable(config.OutputVariable, result);
            }

            return Task.FromResult(new RuleExecutionResult
            {
                Success = true,
                Message = "Calculation completed",
                Data = new { OutputVariable = config.OutputVariable, Result = result }
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new RuleExecutionResult
            {
                Success = false,
                Message = "Calculation failed",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    private Task<RuleExecutionResult> ExecuteSendEmailRule(BusinessRule rule, RuleExecutionContext context)
    {
        var config = JsonSerializer.Deserialize<EmailRuleConfig>(rule.ConfigurationJson, _jsonOptions);
        if (config == null)
            return Task.FromResult(new RuleExecutionResult { Success = false, Message = "Invalid configuration" });

        // For MVP, just simulate email sending
        return Task.FromResult(new RuleExecutionResult
        {
            Success = true,
            Message = $"Email simulated to {config.To}",
            Data = new { To = config.To, Subject = config.Subject }
        });
    }

    private Task<RuleExecutionResult> ExecuteConditionRule(BusinessRule rule, RuleExecutionContext context)
    {
        var config = JsonSerializer.Deserialize<ConditionRuleConfig>(rule.ConfigurationJson, _jsonOptions);
        if (config == null)
            return Task.FromResult(new RuleExecutionResult { Success = false, Message = "Invalid configuration" });

        var leftValue = context.GetVariable(config.LeftOperand)?.ToString() ?? config.LeftOperand;
        var rightValue = context.GetVariable(config.RightOperand)?.ToString() ?? config.RightOperand;

        bool conditionMet = config.Operator switch
        {
            "==" => leftValue == rightValue,
            "!=" => leftValue != rightValue,
            ">" => double.TryParse(leftValue, out var l1) && double.TryParse(rightValue, out var r1) && l1 > r1,
            "<" => double.TryParse(leftValue, out var l2) && double.TryParse(rightValue, out var r2) && l2 < r2,
            ">=" => double.TryParse(leftValue, out var l3) && double.TryParse(rightValue, out var r3) && l3 >= r3,
            "<=" => double.TryParse(leftValue, out var l4) && double.TryParse(rightValue, out var r4) && l4 <= r4,
            "contains" => leftValue.Contains(rightValue, StringComparison.OrdinalIgnoreCase),
            _ => false
        };

        return Task.FromResult(new RuleExecutionResult
        {
            Success = conditionMet,
            Message = conditionMet ? "Condition met" : "Condition not met",
            Data = new { ConditionMet = conditionMet }
        });
    }

    private Task<RuleExecutionResult> ExecuteRestrictionRule(BusinessRule rule, RuleExecutionContext context)
    {
        return Task.FromResult(new RuleExecutionResult
        {
            Success = false,
            Message = "Restricoes verificadas",
            Data = "{}"
        });
    }

    /// <summary>
    /// Converts JsonElement values (from HTTP JSON deserialization) to .NET primitives.
    /// </summary>
    private static object? ExtractJsonValue(object? value)
    {
        if (value is JsonElement je)
        {
            return je.ValueKind switch
            {
                JsonValueKind.String => je.GetString(),
                JsonValueKind.Number => je.TryGetInt64(out var l) ? l : je.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Undefined => null,
                _ => je.GetRawText()
            };
        }
        return value;
    }
}
