using ConferenceApi.Data;
using ConferenceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/v1/conferences/{conferenceId}/talks/{talkId}/votes")]
public class VotesController : ControllerBase
{
    private readonly ConferenceDbContext _db;

    public VotesController(ConferenceDbContext db) => _db = db;

    // GET /api/v1/conferences/{conferenceId}/talks/{talkId}/votes
    [HttpGet]
    [Authorize] // любой авторизованный может смотреть
    public async Task<ActionResult<List<Vote>>> GetVotes(int conferenceId, int talkId)
    {
        var confExists = await _db.Conferences.AnyAsync(c => c.Id == conferenceId);
        if (!confExists)
            return NotFound(new ErrorResponse { Error = "not_found", Message = "Конференция не найдена" });

        var talkExists = await _db.Talks.AnyAsync(t => t.Id == talkId && t.ConferenceId == conferenceId);
        if (!talkExists)
            return NotFound(new ErrorResponse { Error = "not_found", Message = "Доклад не найден" });

        var votes = await _db.Votes
            .Where(v => v.TalkId == talkId)
            .ToListAsync();

        return Ok(votes);
    }

    // POST /api/v1/conferences/{conferenceId}/talks/{talkId}/votes
    [HttpPost]
    [Authorize(Roles = "participant")]
    public async Task<ActionResult<Vote>> CreateVote(int conferenceId, int talkId, [FromBody] Vote vote)
    {
        if (vote == null)
            return BadRequest(new ErrorResponse { Error = "invalid_request", Message = "Тело запроса отсутствует или некорректно" });

        if (vote.ParticipantId <= 0)
            return BadRequest(new ErrorResponse { Error = "validation_error", Message = "ID участника обязателен и должен быть положительным числом" });

        var confExists = await _db.Conferences.AnyAsync(c => c.Id == conferenceId);
        if (!confExists)
            return NotFound(new ErrorResponse { Error = "not_found", Message = "Конференция не найдена" });

        var talkExists = await _db.Talks.AnyAsync(t => t.Id == talkId && t.ConferenceId == conferenceId);
        if (!talkExists)
            return NotFound(new ErrorResponse { Error = "not_found", Message = "Доклад не найден" });

        // Проверка, что участник ещё не голосовал за этот доклад
        var existingVote = await _db.Votes
            .FirstOrDefaultAsync(v => v.TalkId == talkId && v.ParticipantId == vote.ParticipantId);

        if (existingVote != null)
            return Conflict(new ErrorResponse { Error = "already_voted", Message = "Участник уже проголосовал за этот доклад" });

        vote.TalkId = talkId;
        _db.Votes.Add(vote);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetVotes), new { conferenceId, talkId }, vote);
    }
}