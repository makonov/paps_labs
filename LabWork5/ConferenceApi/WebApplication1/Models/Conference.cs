namespace ConferenceApi.Models;

public class Conference
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    public List<Talk> Talks { get; set; } = new();
}