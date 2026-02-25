namespace ConferenceApi.Models;

public class Talk
{
    public int Id { get; set; }
    public int ConferenceId { get; set; }
    public Conference? Conference { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? SpeakerId { get; set; }
    public Speaker? Speaker { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string? Slides { get; set; }
    public List<Vote> Votes { get; set; } = new();
}