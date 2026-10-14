using Microsoft.EntityFrameworkCore;
using R2D2.Application.DTOs;
using R2D2.Application.Interfaces;
using R2D2.Application.RuleEngine;
using R2D2.Domain.Entities;
using R2D2.Domain.Interfaces;

namespace R2D2.Application.Services;

public class WorkflowService : IWorkflowService
{
    private readonly IRepository<Workflow> _workflowRepository;
    private readonly IRepository<WorkflowStep> _stepRepository;
    private readonly IRepository<BusinessRule> _ruleRepository;
    private readonly IRuleExecutionEngine _ruleEngine;

    public WorkflowService(
        IRepository<Workflow> workflowRepository,
        IRepository<WorkflowStep> stepRepository,
        IRepository<BusinessRule> ruleRepository,
        IRuleExecutionEngine ruleEngine)
    {
        _workflowRepository = workflowRepository;
        _stepRepository = stepRepository;
        _ruleRepository = ruleRepository;
        _ruleEngine = ruleEngine;
    }

    public async Task<IEnumerable<WorkflowDto>> GetAllAsync()
    {
        var workflows = await _workflowRepository.Query()
            .Include(w => w.Steps)
            .ThenInclude(s => s.Rule)
            .ToListAsync();

        return workflows.Select(MapToDto);
    }

    public async Task<WorkflowDto?> GetByIdAsync(Guid id)
    {
        var workflow = await _workflowRepository.Query()
            .Include(w => w.Steps)
            .ThenInclude(s => s.Rule)
            .FirstOrDefaultAsync(w => w.Id == id);

        return workflow != null ? MapToDto(workflow) : null;
    }

    public async Task<WorkflowDto> CreateAsync(CreateWorkflowDto dto)
    {
        var workflow = new Workflow
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            FlowJson = dto.FlowJson,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _workflowRepository.AddAsync(workflow);

        if (dto.Steps != null)
        {
            foreach (var stepDto in dto.Steps)
            {
                var step = new WorkflowStep
                {
                    Id = Guid.NewGuid(),
                    WorkflowId = workflow.Id,
                    RuleId = stepDto.RuleId,
                    Order = stepDto.Order,
                    PositionX = stepDto.PositionX,
                    PositionY = stepDto.PositionY,
                    NextStepOnSuccess = stepDto.NextStepOnSuccess,
                    NextStepOnFailure = stepDto.NextStepOnFailure
                };
                await _stepRepository.AddAsync(step);
            }
        }

        return (await GetByIdAsync(workflow.Id))!;
    }

    public async Task<WorkflowDto?> UpdateAsync(Guid id, UpdateWorkflowDto dto)
    {
        var workflow = await _workflowRepository.GetByIdAsync(id);
        if (workflow == null) return null;

        workflow.Name = dto.Name;
        workflow.Description = dto.Description;
        workflow.FlowJson = dto.FlowJson;
        workflow.IsActive = dto.IsActive;
        workflow.UpdatedAt = DateTime.UtcNow;

        await _workflowRepository.UpdateAsync(workflow);
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var workflow = await _workflowRepository.GetByIdAsync(id);
        if (workflow == null) return false;

        await _workflowRepository.DeleteAsync(id);
        return true;
    }

    public async Task<WorkflowStepDto> AddStepAsync(Guid workflowId, CreateWorkflowStepDto dto)
    {
        var rule = await _ruleRepository.GetByIdAsync(dto.RuleId);

        var step = new WorkflowStep
        {
            Id = Guid.NewGuid(),
            WorkflowId = workflowId,
            RuleId = dto.RuleId,
            Order = dto.Order,
            PositionX = dto.PositionX,
            PositionY = dto.PositionY,
            NextStepOnSuccess = dto.NextStepOnSuccess,
            NextStepOnFailure = dto.NextStepOnFailure
        };

        await _stepRepository.AddAsync(step);

        return new WorkflowStepDto(
            step.Id,
            step.WorkflowId,
            step.RuleId,
            rule?.Name ?? "",
            step.Order,
            step.PositionX,
            step.PositionY,
            step.NextStepOnSuccess,
            step.NextStepOnFailure
        );
    }

    public async Task<bool> RemoveStepAsync(Guid workflowId, Guid stepId)
    {
        var step = await _stepRepository.Query()
            .FirstOrDefaultAsync(s => s.WorkflowId == workflowId && s.Id == stepId);

        if (step == null) return false;

        await _stepRepository.DeleteAsync(step.Id);
        return true;
    }

    public async Task<WorkflowExecutionResultDto> ExecuteAsync(Guid workflowId, Dictionary<string, object?> context)
    {
        var workflow = await _workflowRepository.Query()
            .Include(w => w.Steps)
            .ThenInclude(s => s.Rule)
            .FirstOrDefaultAsync(w => w.Id == workflowId);

        if (workflow == null || !workflow.IsActive)
        {
            return new WorkflowExecutionResultDto(false, workflowId, new List<string> { "Workflow not found or inactive" }, new());
        }

        var executionContext = new RuleExecutionContext(context);
        var orderedSteps = workflow.Steps.OrderBy(s => s.Order).ToList();
        var allErrors = new List<string>();
        bool workflowSuccess = true;

        foreach (var step in orderedSteps)
        {
            if (!step.Rule.IsActive) continue;

            var result = await _ruleEngine.ExecuteRuleAsync(step.Rule, executionContext);
            executionContext.AddStepResult(step.Id.ToString(), result);

            if (!result.Success)
            {
                allErrors.AddRange(result.Errors);
                
                if (string.IsNullOrEmpty(step.NextStepOnFailure))
                {
                    // If no failure path is defined, we stop the workflow and report failure
                    workflowSuccess = false;
                    break;
                }
                
                // If NextStepOnFailure is handled (not yet fully implemented in engine, but logically here), 
                // we might continue. For now, we assume interruption if it's a critical failure.
            }
        }

        return new WorkflowExecutionResultDto(
            workflowSuccess,
            workflowId,
            allErrors,
            executionContext.StepResults
        );
    }

    private static WorkflowDto MapToDto(Workflow workflow) => new(
        workflow.Id,
        workflow.Name,
        workflow.Description,
        workflow.FlowJson,
        workflow.IsActive,
        workflow.CreatedAt,
        workflow.UpdatedAt,
        workflow.Steps.OrderBy(s => s.Order).Select(s => new WorkflowStepDto(
            s.Id,
            s.WorkflowId,
            s.RuleId,
            s.Rule?.Name ?? "",
            s.Order,
            s.PositionX,
            s.PositionY,
            s.NextStepOnSuccess,
            s.NextStepOnFailure
        )).ToList()
    );
}
