using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R2D2.Application.DTOs;
using R2D2.Application.Interfaces;

namespace R2D2.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ListViewsController : ControllerBase
{
    private readonly IListViewService _service;

    public ListViewsController(IListViewService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ListViewDto>>> GetAll()
    {
        var views = await _service.GetAllAsync();
        return Ok(views);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ListViewDto>> GetById(Guid id)
    {
        var view = await _service.GetByIdAsync(id);
        if (view == null) return NotFound();
        return Ok(view);
    }

    [HttpPost]
    public async Task<ActionResult<ListViewDto>> Create(CreateListViewDto dto)
    {
        var view = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = view.Id }, view);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ListViewDto>> Update(Guid id, UpdateListViewDto dto)
    {
        var view = await _service.UpdateAsync(id, dto);
        if (view == null) return NotFound();
        return Ok(view);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{listViewId:guid}/columns")]
    public async Task<ActionResult<ListViewColumnDto>> AddColumn(Guid listViewId, CreateListViewColumnDto dto)
    {
        var column = await _service.AddColumnAsync(listViewId, dto);
        return Ok(column);
    }

    [HttpDelete("{listViewId:guid}/columns/{columnId:guid}")]
    public async Task<ActionResult> RemoveColumn(Guid listViewId, Guid columnId)
    {
        var result = await _service.RemoveColumnAsync(listViewId, columnId);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPut("{listViewId:guid}/columns")]
    public async Task<ActionResult> ReplaceColumns(Guid listViewId, List<CreateListViewColumnDto> columns)
    {
        var result = await _service.ReplaceColumnsAsync(listViewId, columns);
        if (!result) return NotFound();
        return Ok();
    }
}
