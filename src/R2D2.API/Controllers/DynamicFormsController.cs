using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R2D2.Application.DTOs;
using R2D2.Application.Interfaces;

namespace R2D2.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DynamicFormsController : ControllerBase
{
    private readonly IDynamicFormService _service;
    private readonly IValidator<CreateDynamicFormDto> _createValidator;

    public DynamicFormsController(
        IDynamicFormService service,
        IValidator<CreateDynamicFormDto> createValidator)
    {
        _service = service;
        _createValidator = createValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DynamicFormDto>>> GetAll()
    {
        var forms = await _service.GetAllAsync();
        return Ok(forms);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DynamicFormDto>> GetById(Guid id)
    {
        var form = await _service.GetByIdAsync(id);
        if (form == null) return NotFound();
        return Ok(form);
    }

    [HttpPost]
    public async Task<ActionResult<DynamicFormDto>> Create(CreateDynamicFormDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var form = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = form.Id }, form);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DynamicFormDto>> Update(Guid id, UpdateDynamicFormDto dto)
    {
        var form = await _service.UpdateAsync(id, dto);
        if (form == null) return NotFound();
        return Ok(form);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{formId:guid}/fields")]
    public async Task<ActionResult<FormFieldDto>> AddField(Guid formId, CreateFormFieldDto dto)
    {
        var field = await _service.AddFieldAsync(formId, dto);
        return Ok(field);
    }

    [HttpDelete("{formId:guid}/fields/{formFieldId:guid}")]
    public async Task<ActionResult> RemoveField(Guid formId, Guid formFieldId)
    {
        var result = await _service.RemoveFieldAsync(formId, formFieldId);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPut("{formId:guid}/fields/order")]
    public async Task<ActionResult> UpdateFieldOrder(Guid formId, [FromBody] List<Guid> fieldOrder)
    {
        await _service.UpdateFieldOrderAsync(formId, fieldOrder);
        return NoContent();
    }

    [HttpPut("{formId:guid}/fields/{formFieldId:guid}")]
    public async Task<ActionResult<FormFieldDto>> UpdateField(Guid formId, Guid formFieldId, UpdateFormFieldDto dto)
    {
        var field = await _service.UpdateFieldAsync(formId, formFieldId, dto);
        if (field == null) return NotFound();
        return Ok(field);
    }
}
