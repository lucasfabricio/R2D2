using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using R2D2.Application.DTOs;
using R2D2.Application.Interfaces;

namespace R2D2.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TableDataController : ControllerBase
{
    private readonly ITableDataService _service;

    public TableDataController(ITableDataService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get all data for a specific list
    /// </summary>
    [HttpGet("list/{listId:guid}")]
    public async Task<ActionResult<IEnumerable<TableDataDto>>> GetByListId(Guid listId)
    {
        var data = await _service.GetByListIdAsync(listId);
        return Ok(data);
    }

    /// <summary>
    /// Get a specific data record by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TableDataDto>> GetById(Guid id)
    {
        var data = await _service.GetByIdAsync(id);
        if (data == null) return NotFound();
        return Ok(data);
    }

    /// <summary>
    /// Create a new data record
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TableDataDto>> Create(CreateTableDataDto dto)
    {
        var data = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = data.Id }, data);
    }

    /// <summary>
    /// Update an existing data record
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TableDataDto>> Update(Guid id, UpdateTableDataDto dto)
    {
        var data = await _service.UpdateAsync(id, dto);
        if (data == null) return NotFound();
        return Ok(data);
    }

    /// <summary>
    /// Delete a data record
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
