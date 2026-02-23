using ConferenceApi.Data;
using ConferenceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/v1/conferences/{conferenceId}/schedule")]
public class ScheduleController : ControllerBase
{
    private readonly ConferenceDbContext _db;

    public ScheduleController(ConferenceDbContext db) => _db = db;

    // GET /api/v1/conferences/{conferenceId}/schedule
    [HttpGet]
    [AllowAnonymous] // публичный доступ
    public async Task<ActionResult<List<Talk>>> GetSchedule(int conferenceId)
    {
        // Проверка существования конференции
        var confExists = await _db.Conferences.AnyAsync(c => c.Id == conferenceId);
        if (!confExists)
            return NotFound(new ErrorResponse { Error = "not_found", Message = "Конференция не найдена" });

        // Получаем расписание докладов для конференции, сортируем по StartTime
        var schedule = await _db.Talks
            .Where(t => t.ConferenceId == conferenceId)
            .OrderBy(t => t.StartTime)
            .ToListAsync();

        return Ok(schedule);
    }
}