using ConferenceApi.Data;
using ConferenceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/v1/speakers")]
public class SpeakersController : ControllerBase
{
    private readonly ConferenceDbContext _db;

    public SpeakersController(ConferenceDbContext db) => _db = db;

    // GET /api/v1/speakers?page=1&limit=20
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<Speaker>>> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var speakers = await _db.Speakers
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return Ok(speakers);
    }

    // GET /api/v1/speakers/{id}
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Speaker>> Get(int id)
    {
        var speaker = await _db.Speakers.FindAsync(id);
        if (speaker == null)
            return NotFound(new ErrorResponse { Error = "not_found", Message = "Спикер не найден" });

        return Ok(speaker);
    }

    // POST /api/v1/speakers
    [HttpPost]
    [Authorize(Roles = "organizer")]
    public async Task<ActionResult<Speaker>> Create([FromBody] Speaker speaker)
    {
        if (string.IsNullOrWhiteSpace(speaker.Name))
            return BadRequest(new ErrorResponse { Error = "validation_error", Message = "Имя спикера обязательно" });

        _db.Speakers.Add(speaker);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = speaker.Id }, speaker);
    }

    // PUT /api/v1/speakers/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "organizer")]
    public async Task<ActionResult<Speaker>> Update(int id, [FromBody] Speaker updatedSpeaker)
    {
        var existing = await _db.Speakers.FindAsync(id);
        if (existing == null)
            return NotFound(new ErrorResponse { Error = "not_found", Message = "Спикер не найден" });

        existing.Name = updatedSpeaker.Name;
        existing.Bio = updatedSpeaker.Bio;

        await _db.SaveChangesAsync();
        return Ok(existing);
    }

    // DELETE /api/v1/speakers/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "organizer")]
    public async Task<IActionResult> Delete(int id)
    {
        var speaker = await _db.Speakers.FindAsync(id);
        if (speaker != null)
        {
            _db.Speakers.Remove(speaker);
            await _db.SaveChangesAsync();
        }

        // Идемпотентно — возвращаем 204 даже если спикера не было
        return NoContent();
    }
}