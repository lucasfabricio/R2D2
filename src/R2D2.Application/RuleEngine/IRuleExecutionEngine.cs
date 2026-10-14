using R2D2.Domain.Entities;

namespace R2D2.Application.RuleEngine;

public interface IRuleExecutionEngine
{
    Task<RuleExecutionResult> ExecuteRuleAsync(BusinessRule rule, RuleExecutionContext context);
}
