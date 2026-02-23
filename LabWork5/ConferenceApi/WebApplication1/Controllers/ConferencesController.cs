using ConferenceApi.Data;
using ConferenceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/v1/conferences")]
public class ConferencesController : ControllerBase
{
    private readonly ConferenceDbContext _db;

    public ConferencesController(ConferenceDbContext db) => _db = db;

    // GET /api/v1/conferences?page=1&limit=20
    [HttpGet]
    [AllowAnonymous] // публичный список
    public async Task<ActionResult<List<Conference>>> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var list = await _db.Conferences
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return Ok(list);
    }

    // GET /api/v1/conferences/{id}
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Conference>> Get(int id)
    {
        var conf = await _db.Conferences.FindAsync(id);
        if (conf == null)
            return NotFound(new ErrorResponse { Error = "not_found", Message = "Конференция не найдена" });

        return Ok(conf);
    }

    // POST /api/v1/conferences
    [HttpPost]
    [Authorize(Roles = "organizer")]
    public async Task<ActionResult<Conference>> Create([FromBody] Conference conf)
    {
        // 1. Проверка тела запроса
        if (conf == null)
            return BadRequest(new ErrorResponse { Error = "invalid_request", Message = "Тело запроса отсутствует или некорректно" });

        // 2. Проверка обязательных полей
        if (string.IsNullOrWhiteSpace(conf.Name))
            return BadRequest(new ErrorResponse { Error = "validation_error", Message = "Название конференции обязательно" });

        if (string.IsNullOrWhiteSpace(conf.StartDate))
            return BadRequest(new ErrorResponse { Error = "validation_error", Message = "Дата начала конференции обязательна" });

        if (string.IsNullOrWhiteSpace(conf.EndDate))
            return BadRequest(new ErrorResponse { Error = "validation_error", Message = "Дата окончания конференции обязательна" });

        if (string.IsNullOrWhiteSpace(conf.Location))
            return BadRequest(new ErrorResponse { Error = "validation_error", Message = "Место проведения конференции обязательно" });

        // 3. Проверка логики дат
        if (!DateTime.TryParse(conf.StartDate, out var start) ||
            !DateTime.TryParse(conf.EndDate, out var end))
            return BadRequest(new ErrorResponse { Error = "validation_error", Message = "Некорректный формат дат (YYYY-MM-DD)" });

        if (start >= end)
            return BadRequest(new ErrorResponse { Error = "validation_error", Message = "Дата начала должна быть раньше даты окончания" });

        // 4. Сохранение в БД
        _db.Conferences.Add(conf);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = conf.Id }, conf);
    }

    // PUT /api/v1/conferences/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "organizer")]
    public async Task<ActionResult<Conference>> Update(int id, [FromBody] Conference conf)
    {
        var existing = await _db.Conferences.FindAsync(id);
        if (existing == null)
            return NotFound(new ErrorResponse { Error = "not_found", Message = "Конференция не найдена" });

        // Обновляем поля
        existing.Name = conf.Name;
        existing.StartDate = conf.StartDate;
        existing.EndDate = conf.EndDate;
        existing.Location = conf.Location;

        await _db.SaveChangesAsync();
        return Ok(existing);
    }

    // DELETE /api/v1/conferences/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "organizer")]
    public async Task<IActionResult> Delete(int id)
    {
        var conf = await _db.Conferences.FindAsync(id);
        if (conf != null)
        {
            _db.Conferences.Remove(conf);
            await _db.SaveChangesAsync();
        }

        // идемпотентно — возвращаем 204 даже если конференция не найдена
        return NoContent();
    }
}