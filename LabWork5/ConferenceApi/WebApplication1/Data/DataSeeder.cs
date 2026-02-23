using ConferenceApi.Models;

namespace ConferenceApi.Data;

public static class DataSeeder
{
    public static void Seed(ConferenceDbContext db)
    {
        if (db.Conferences.Any()) return;

        var speaker = new Speaker
        {
            Name = "Анна Смирнова",
            Bio = "Разработчик .NET и ASP.NET Core"
        };
        db.Speakers.Add(speaker);
        db.SaveChanges();

        var conf = new Conference
        {
            Name = "DevConf Spring 2026",
            StartDate = "2026-04-15",
            EndDate = "2026-04-17",
            Location = "Санкт-Петербург"
        };
        db.Conferences.Add(conf);
        db.SaveChanges();

        var talk1 = new Talk
        {
            ConferenceId = conf.Id,
            Title = "JWT авторизация в ASP.NET Core",
            SpeakerId = speaker.Id,
            StartTime = "2026-04-15T11:00",
            Room = "Зал 2",
            Slides = "https://slides.com/jwt-asp"
        };
        var talk2 = new Talk
        {
            ConferenceId = conf.Id,
            Title = "Тестирование REST API с Postman",
            SpeakerId = speaker.Id,
            StartTime = "2026-04-15T14:00",
            Room = "Зал 3",
            Slides = "https://example.com/postman-testing.pdf"
        };
        db.Talks.AddRange(talk1, talk2);
        db.SaveChanges();

        var vote1 = new Vote { TalkId = talk1.Id, ParticipantId = 1001, Value = true };
        var vote2 = new Vote { TalkId = talk1.Id, ParticipantId = 1002, Value = false };
        db.Votes.AddRange(vote1, vote2);
        db.SaveChanges();

        Console.WriteLine("Сид-данные загружены в PostgreSQL: 1 спикер, 1 конференция, 2 доклада, 2 голоса.");
    }
}