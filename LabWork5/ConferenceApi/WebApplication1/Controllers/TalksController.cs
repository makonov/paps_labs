using ConferenceApi.Data;
using ConferenceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/v1/conferences/{conferenceId}/talks")]
public class TalksController : ControllerBase
{
    private readonly ConferenceDbContext _db;

    public TalksController(ConferenceDbContext db) => _db = db;

    // GET /api/v1/conferences/{conferenceId}/talks?page=1&limit=20&speakerId=5
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<Talk>>> GetTalks(int conferenceId, [FromQuery] int page = 1, [FromQuery] int limit = 20, [FromQuery] int? speakerId = null)
    {
        var confExists = await _db.Conferences.AnyAsync(c => c.Id == conferenceId);
        if (!confExists)
            return NotFound(new ErrorResponse { Error = "not_found", Message = "Конференция не найдена" });

        var query = _db.Talks.Where(t => t.ConferenceId == conferenceId);

        if (speakerId.HasValue)
            query = query.Where(t => t.SpeakerId == speakerId.Value);

        var talks = await query
            .OrderBy(t => t.StartTime)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return Ok(talks);
    }

    // GET /api/v1/conferences/{conferenceId}/talks/{talkId}
    [HttpGet("{talkId}")]
    [AllowAnonymous]
    public async Task<ActionResult<Talk>> GetTalk(int conferenceId, int talkId)
    {
        var talk = await _db.Talks.FirstOrDefaultAsync(t => t.Id == talkId && t.ConferenceId == conferenceId);
        if (talk == null)
            return NotFound(new ErrorResponse { Error = "not_found", Message = "Доклад не найден" });

        return Ok(talk);
    }

    // POST /api/v1/conferences/{conferenceId}/talks
    [HttpPost]
    [Authorize(Roles = "speaker")]
    public async Task<ActionResult<Talk>> Create(int conferenceId, [FromBody] Talk talk)
    {
        if (talk == null)
            return BadRequest(new ErrorResponse { Error = "invalid_request", Message = "Тело запроса отсутствует или некорректно" });

        if (string.IsNullOrWhiteSpace(talk.Title))
            return BadRequest(new ErrorResponse { Error = "validation_error", Message = "Название доклада обязательно" });

        if (talk.SpeakerId <= 0)
            return BadRequest(new ErrorResponse { Error = "validation_error", Message = "ID спикера обязателен и должен быть положительным числом" });

        if (string.IsNullOrWhiteSpace(talk.StartTime))
            return BadRequest(new ErrorResponse { Error = "validation_error", Message = "Время начала доклада обязательно" });

        if (string.IsNullOrWhiteSpace(talk.Room))
            return BadRequest(new ErrorResponse { Error = "validation_error", Message = "Аудитория (комната) обязательна" });

        // Проверка конференции
        var confExists = await _db.Conferences.AnyAsync(c => c.Id == conferenceId);
        if (!confExists)
            return NotFound(new ErrorResponse { Error = "not_found", Message = "Конференция не найдена" });

        // Проверка спикера
        var speakerExists = await _db.Speakers.AnyAsync(s => s.Id == talk.SpeakerId);
        if (!speakerExists)
            return NotFound(new ErrorResponse { Error = "not_found", Message = "Спикер с указанным ID не найден" });

        // Проверка формата времени
        if (!DateTime.TryParse(talk.StartTime, out var _))
            return BadRequest(new ErrorResponse { Error = "validation_error", Message = "Некорректный формат времени начала (ожидается YYYY-MM-DDTHH:MM)" });

        talk.ConferenceId = conferenceId;
        _db.Talks.Add(talk);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTalk), new { conferenceId, talkId = talk.Id }, talk);
    }

    // PUT /api/v1/conferences/{conferenceId}/talks/{talkId}
    [HttpPut("{talkId}")]
    [Authorize(Roles = "speaker")]
    public async Task<ActionResult<Talk>> Update(int conferenceId, int talkId, [FromBody] Talk updatedTalk)
    {
        if (updatedTalk == null)
            return BadRequest(new ErrorResponse { Error = "invalid_request", Message = "Тело запроса отсутствует или некорректно" });

        if (string.IsNullOrWhiteSpace(updatedTalk.Title))
            return BadRequest(new ErrorResponse { Error = "validation_error", Message = "Название доклада обязательно" });

        if (updatedTalk.SpeakerId <= 0)
            return BadRequest(new ErrorResponse { Error = "validation_error", Message = "ID спикера обязателен и должен быть положительным числом" });

        if (string.IsNullOrWhiteSpace(updatedTalk.StartTime))
            return BadRequest(new ErrorResponse { Error = "validation_error", Message = "Время начала доклада обязательно" });

        if (string.IsNullOrWhiteSpace(updatedTalk.Room))
            return BadRequest(new ErrorResponse { Error = "validation_error", Message = "Аудитория (комната) обязательна" });

        // Проверка существования конференции и доклада
        var talk = await _db.Talks.FirstOrDefaultAsync(t => t.Id == talkId && t.ConferenceId == conferenceId);
        if (talk == null)
            return NotFound(new ErrorResponse { Error = "not_found", Message = "Доклад не найден" });

        var speakerExists = await _db.Speakers.AnyAsync(s => s.Id == updatedTalk.SpeakerId);
        if (!speakerExists)
            return NotFound(new ErrorResponse { Error = "not_found", Message = "Спикер с указанным ID не найден" });

        if (!DateTime.TryParse(updatedTalk.StartTime, out var _))
            return BadRequest(new ErrorResponse { Error = "validation_error", Message = "Некорректный формат времени начала (ожидается YYYY-MM-DDTHH:MM)" });

        // Обновляем поля
        talk.Title = updatedTalk.Title;
        talk.SpeakerId = updatedTalk.SpeakerId;
        talk.StartTime = updatedTalk.StartTime;
        talk.Room = updatedTalk.Room;
        talk.Slides = updatedTalk.Slides;

        await _db.SaveChangesAsync();
        return Ok(talk);
    }

    // DELETE /api/v1/conferences/{conferenceId}/talks/{talkId}
    [HttpDelete("{talkId}")]
    [Authorize(Roles = "speaker")]
    public async Task<IActionResult> Delete(int conferenceId, int talkId)
    {
        var talk = await _db.Talks.FirstOrDefaultAsync(t => t.Id == talkId && t.ConferenceId == conferenceId);
        if (talk != null)
        {
            _db.Talks.Remove(talk);
            await _db.SaveChangesAsync();
        }

        return NoContent(); // идемпотентно
    }
}