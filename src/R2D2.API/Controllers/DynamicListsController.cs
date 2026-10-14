using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R2D2.Application.DTOs;
using R2D2.Application.Interfaces;

namespace R2D2.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DynamicListsController : ControllerBase
{
    private readonly IDynamicListService _service;
    private readonly IValidator<CreateDynamicListDto> _createValidator;

    public DynamicListsController(
        IDynamicListService service,
        IValidator<CreateDynamicListDto> createValidator)
    {
        _service = service;
        _createValidator = createValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DynamicListDto>>> GetAll()
    {
        var lists = await _service.GetAllAsync();
        return Ok(lists);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DynamicListDto>> GetById(Guid id)
    {
        var list = await _service.GetByIdAsync(id);
        if (list == null) return NotFound();
        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<DynamicListDto>> Create(CreateDynamicListDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var list = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = list.Id }, list);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DynamicListDto>> Update(Guid id, UpdateDynamicListDto dto)
    {
        var list = await _service.UpdateAsync(id, dto);
        if (list == null) return NotFound();
        return Ok(list);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{listId:guid}/fields")]
    public async Task<ActionResult<DynamicFieldDto>> AddField(Guid listId, CreateDynamicFieldDto dto)
    {
        var field = await _service.AddFieldAsync(listId, dto);
        return Ok(field);
    }

    [HttpDelete("{listId:guid}/fields/{fieldId:guid}")]
    public async Task<ActionResult> RemoveField(Guid listId, Guid fieldId)
    {
        var result = await _service.RemoveFieldAsync(listId, fieldId);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPut("{listId:guid}/fields/{fieldId:guid}")]
    public async Task<ActionResult<DynamicFieldDto>> UpdateField(Guid listId, Guid fieldId, UpdateDynamicFieldDto dto)
    {
        var field = await _service.UpdateFieldAsync(listId, fieldId, dto);
        if (field == null) return NotFound();
        return Ok(field);
    }
}
