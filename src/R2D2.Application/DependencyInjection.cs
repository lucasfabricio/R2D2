using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using R2D2.Application.Interfaces;
using R2D2.Application.RuleEngine;
using R2D2.Application.Services;
using R2D2.Application.Validators;

namespace R2D2.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<IDynamicListService, DynamicListService>();
        services.AddScoped<IDynamicFormService, DynamicFormService>();
        services.AddScoped<IBusinessRuleService, BusinessRuleService>();
        services.AddScoped<IWorkflowService, WorkflowService>();
        services.AddScoped<ITableDataService, TableDataService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IListViewService, ListViewService>();

        // Register rule engine
        services.AddScoped<IRuleExecutionEngine, RuleExecutionEngine>();

        // Register validators
        services.AddValidatorsFromAssemblyContaining<CreateDynamicFieldValidator>();

        return services;
    }
}
