using ConferenceApi.Data;
using ConferenceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/v1/conferences/{conferenceId}/talks/{talkId}/slides")]
public class SlidesController : ControllerBase
{
    private readonly ConferenceDbContext _db;

    public SlidesController(ConferenceDbContext db) => _db = db;

    // GET /api/v1/conferences/{conferenceId}/talks/{talkId}/slides
    [HttpGet]
    [Authorize(Roles = "participant")]
    public async Task<ActionResult<object>> GetSlides(int conferenceId, int talkId)
    {
        // Проверка существования конференции
        var confExists = await _db.Conferences.AnyAsync(c => c.Id == conferenceId);
        if (!confExists)
            return NotFound(new ErrorResponse { Error = "not_found", Message = "Конференция не найдена" });

        // Получаем доклад
        var talk = await _db.Talks
            .Where(t => t.Id == talkId && t.ConferenceId == conferenceId)
            .FirstOrDefaultAsync();

        if (talk == null)
            return NotFound(new ErrorResponse { Error = "not_found", Message = "Доклад не найден" });

        if (string.IsNullOrEmpty(talk.Slides))
            return NotFound(new ErrorResponse { Error = "not_found", Message = "Слайды не загружены" });

        // Для лабы возвращаем просто ссылку
        return Ok(new { SlidesUrl = talk.Slides, Message = "Доступ разрешён (только для участников)" });
    }
}