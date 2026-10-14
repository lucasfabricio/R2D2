using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R2D2.Application.DTOs;
using R2D2.Application.Interfaces;

namespace R2D2.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BusinessRulesController : ControllerBase
{
    private readonly IBusinessRuleService _service;
    private readonly IValidator<CreateBusinessRuleDto> _createValidator;

    public BusinessRulesController(
        IBusinessRuleService service,
        IValidator<CreateBusinessRuleDto> createValidator)
    {
        _service = service;
        _createValidator = createValidator;
    }

    /// <summary>
    /// Get all business rules
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BusinessRuleDto>>> GetAll()
    {
        var rules = await _service.GetAllAsync();
        return Ok(rules);
    }

    /// <summary>
    /// Get a business rule by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BusinessRuleDto>> GetById(Guid id)
    {
        var rule = await _service.GetByIdAsync(id);
        if (rule == null) return NotFound();
        return Ok(rule);
    }

    /// <summary>
    /// Create a new business rule
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BusinessRuleDto>> Create(CreateBusinessRuleDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var rule = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = rule.Id }, rule);
    }

    /// <summary>
    /// Update an existing business rule
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BusinessRuleDto>> Update(Guid id, UpdateBusinessRuleDto dto)
    {
        var rule = await _service.UpdateAsync(id, dto);
        if (rule == null) return NotFound();
        return Ok(rule);
    }

    /// <summary>
    /// Delete a business rule
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
