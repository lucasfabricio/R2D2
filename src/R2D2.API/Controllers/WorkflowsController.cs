using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R2D2.Application.DTOs;
using R2D2.Application.Interfaces;

namespace R2D2.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowService _service;
    private readonly IValidator<CreateWorkflowDto> _createValidator;

    public WorkflowsController(
        IWorkflowService service,
        IValidator<CreateWorkflowDto> createValidator)
    {
        _service = service;
        _createValidator = createValidator;
    }

    /// <summary>
    /// Get all workflows
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkflowDto>>> GetAll()
    {
        var workflows = await _service.GetAllAsync();
        return Ok(workflows);
    }

    /// <summary>
    /// Get a workflow by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkflowDto>> GetById(Guid id)
    {
        var workflow = await _service.GetByIdAsync(id);
        if (workflow == null) return NotFound();
        return Ok(workflow);
    }

    /// <summary>
    /// Create a new workflow
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<WorkflowDto>> Create(CreateWorkflowDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var workflow = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = workflow.Id }, workflow);
    }

    /// <summary>
    /// Update an existing workflow
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WorkflowDto>> Update(Guid id, UpdateWorkflowDto dto)
    {
        var workflow = await _service.UpdateAsync(id, dto);
        if (workflow == null) return NotFound();
        return Ok(workflow);
    }

    /// <summary>
    /// Delete a workflow
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Add a step to a workflow
    /// </summary>
    [HttpPost("{workflowId:guid}/steps")]
    public async Task<ActionResult<WorkflowStepDto>> AddStep(Guid workflowId, CreateWorkflowStepDto dto)
    {
        var step = await _service.AddStepAsync(workflowId, dto);
        return Ok(step);
    }

    /// <summary>
    /// Remove a step from a workflow
    /// </summary>
    [HttpDelete("{workflowId:guid}/steps/{stepId:guid}")]
    public async Task<ActionResult> RemoveStep(Guid workflowId, Guid stepId)
    {
        var result = await _service.RemoveStepAsync(workflowId, stepId);
        if (!result) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Execute a workflow
    /// </summary>
    [HttpPost("{id:guid}/execute")]
    public async Task<ActionResult<WorkflowExecutionResultDto>> Execute(Guid id, [FromBody] Dictionary<string, object?> context)
    {
        var result = await _service.ExecuteAsync(id, context);
        return Ok(result);
    }
}
