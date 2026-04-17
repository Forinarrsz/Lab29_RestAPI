using System.Collections;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using TaskApi.Models;
namespace TaskApi.Controllers;


[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private static List<TaskItem> _tasks = new()
    {
        new TaskItem
        {
            Id = 1,
            Title = "Learn ASP.NET Core",
            Priority = "high",
            IsCompleted = true

        },
        new TaskItem
        {
            Id = 2,
            Title = "complete Lab28",
            Priority = "high",
            IsCompleted = false

        },
        new TaskItem
        {
            Id = 3,
            Title = "Write README",
            Priority = "Normal",
            IsCompleted = false

        },
    };
    private static int _nextId = 4;
    [HttpGet]
    public ActionResult<IEnumerable<TaskItem>> GetAll([FromQuery] bool? completed = null)
    {
        var result = _tasks.AsEnumerable();

        if (completed.HasValue)
            result = result.Where(t => t.IsCompleted == completed.Value);
        return Ok(result);
    }


    [HttpGet("{id}")]
    public ActionResult<TaskItem> GetById(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task is null)
            return NotFound(new { Message = $"Task with ID = {id} not found" });
        return Ok(task);
    }

    [HttpPost]
    public ActionResult<TaskItem> Create([FromBody] CreateTaskkDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { Message = "Enter Title" });
        var newTask = new TaskItem
        {
            Id = _nextId++,
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            IsCompleted = false,
            CreatedAt = DateTime.Now

        };
        _tasks.Add(newTask);
        return CreatedAtAction(nameof(GetById), new { id = newTask.Id }, newTask);
    }

    [HttpPut("{id}")]
    public ActionResult<TaskItem> Update(int id, [FromBody] UpdateTaskDto dto)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);

        if (task is null)
            return NotFound(new { Message = $"Task with ID = {id} not found" });
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { Message = "Enter Title" });
        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Priority = dto.Priority;
        task.IsCompleted = dto.IsCompleted;
        return Ok(task);
    }

    [HttpDelete("{id}")]
    public ActionResult<TaskItem> Delete(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);

        if (task is null)
            return NotFound(new { Message = $"Task with ID = {id} not found" });
        _tasks.Remove(task);

        return NoContent();

    }

    [HttpPatch("{id}/complete")]
    public ActionResult<TaskItem> MarkComplete(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);

        if (task is null)
            return NotFound(new { Message = $"Task with ID = {id} not found" });
        task.IsCompleted = !task.IsCompleted;
        return Ok(task);

    }
    [HttpGet("search")]
    public ActionResult<IEnumerable<TaskItem>> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return BadRequest(new { Message = "query not be empty" });

        var results = _tasks.Where(t => t.Title.Contains(query, StringComparison.OrdinalIgnoreCase) || t.Description.Contains(query, StringComparison.InvariantCultureIgnoreCase))
        .ToList();

        return Ok(results);
    }

    [HttpGet("stats")]
    public ActionResult GetStats()
    {
        var total = _tasks.Count;
        var completed = _tasks.Count(t => t.IsCompleted);
        var pending = total - completed;

        var stats = new
        {
            Total = total,
            Completed = completed,
            Pending = pending,
            CompletionPct = total > 0 ? Math.Round((double)completed / total * 100, 1) : 0,
            ByPriority = new
            {
                High = _tasks.Count(t => t.Priority == "High"),
                Normal = _tasks.Count(t => t.Priority == "Normal"),
                Low = _tasks.Count(t => t.Priority == "Low"),
            }
        };
        return Ok(stats);
    }
    [HttpGet("sorted")]
    public ActionResult<IEnumerable<TaskItem>> GetSorted
    (
        [FromQuery] string by = "id",

        [FromQuery] bool desc = false
    )
    {
        IEnumerable<TaskItem> sorted = by.ToLower() switch
        {
            "title" => _tasks.OrderBy(t => t.Title),
            "priority" => _tasks.OrderBy(t => t.Priority),
            "createdat" => _tasks.OrderBy(t => t.CreatedAt),
            _ => _tasks.OrderBy(t => t.Id),
        };
        if (desc)
            sorted = sorted.Reverse();
        return Ok(sorted);
    }
}