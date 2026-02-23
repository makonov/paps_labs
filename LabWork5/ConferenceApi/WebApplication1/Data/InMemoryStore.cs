using ConferenceApi.Models;

namespace ConferenceApi.Data;

public static class InMemoryStore
{
    public static readonly Dictionary<int, Conference> Conferences = new();
    public static readonly Dictionary<int, Talk> Talks = new();
    public static readonly Dictionary<int, Speaker> Speakers = new();
    public static readonly Dictionary<int, Vote> Votes = new();

    private static int _nextConferenceId = 1;
    private static int _nextTalkId = 1;
    private static int _nextSpeakerId = 1;
    private static int _nextVoteId = 1;

    public static int NextConferenceId() => _nextConferenceId++;
    public static int NextTalkId() => _nextTalkId++;
    public static int NextSpeakerId() => _nextSpeakerId++;
    public static int NextVoteId() => _nextVoteId++;
}