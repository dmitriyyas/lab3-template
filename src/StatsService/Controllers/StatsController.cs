using Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StatsService.Dto;
using StatsService.Models;

namespace StatsService.Controllers;

[ApiController]
[Route("")]
public class StatsController(StatsContext context) : ControllerBase
{
    private readonly StatsContext _context = context;

    [HttpGet("api/v1/stats")]
    [Authorize]
    public async Task<IActionResult> GetStats([FromQuery] int? page = null, [FromQuery] int? size = null)
    {
        if (!HttpContext.IsAdminRequest())
            return Forbid();

        IQueryable<Stat> query = _context.Stats;

        query = query.OrderByDescending(s => s.Timestamp);

        var totalElements = await query.CountAsync();

        if (page is not null && size is not null)
            query = query.Skip((page.Value - 1) * size.Value).Take(size.Value);

        var stats = await query.ToListAsync();

        var result = new StatsDto(page ?? 1,
            size ?? totalElements,
            totalElements,
            stats.Select(s => ToDto(s)).ToList());

        return Ok(result);
    }

    private static StatDto ToDto(Stat stat)
    {
        return new StatDto(id: stat.Id,
            method: stat.Method,
            path: stat.Path,
            statusCode: stat.StatusCode,
            durationMs: stat.DurationMs,
            timestamp: stat.Timestamp);
    }
}
