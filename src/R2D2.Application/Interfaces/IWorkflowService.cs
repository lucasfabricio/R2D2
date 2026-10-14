using R2D2.Application.DTOs;

namespace R2D2.Application.Interfaces;

public interface IWorkflowService
{
    Task<IEnumerable<WorkflowDto>> GetAllAsync();
    Task<WorkflowDto?> GetByIdAsync(Guid id);
    Task<WorkflowDto> CreateAsync(CreateWorkflowDto dto);
    Task<WorkflowDto?> UpdateAsync(Guid id, UpdateWorkflowDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<WorkflowStepDto> AddStepAsync(Guid workflowId, CreateWorkflowStepDto dto);
    Task<bool> RemoveStepAsync(Guid workflowId, Guid stepId);
    Task<WorkflowExecutionResultDto> ExecuteAsync(Guid workflowId, Dictionary<string, object?> context);
}
