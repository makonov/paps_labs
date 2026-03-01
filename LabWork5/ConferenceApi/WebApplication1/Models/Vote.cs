namespace ConferenceApi.Models;

public class Vote
{
    public int? Id { get; set; }
    public int? TalkId { get; set; }
    public Talk? Talk { get; set; } = null!;
    public int? ParticipantId { get; set; }
    public bool Value { get; set; }
}