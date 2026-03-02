# Лабораторная работа №6  
## Тема: Использование шаблонов проектирования  

## Порождающие шаблоны  
### Abstract Factory (Абстрактная фабрика)

## 1. Общее назначение шаблона

Шаблон **Abstract Factory** относится к порождающим шаблонам GoF и предназначен для создания семейств взаимосвязанных объектов без указания их конкретных классов.  

Он позволяет:
- изолировать процесс создания объектов;  
- обеспечивать согласованность создаваемых компонентов;  
- заменить целое семейство объектов одной строкой кода;  
- соблюдать принцип открытости/закрытости (Open/Closed Principle).


## 2. Применение в системе управления конференциями

В системе существуют разные типы конференций:  
- Онлайн  
- Гибрид  

Каждый тип конференции использует согласованный набор компонентов:  
- Расписание  
- Сервис уведомлений  
- Генератор отчетов  

Использование шаблона **Abstract Factory** позволяет:  
- создавать согласованный набор компонентов;  
- не привязывать клиентский код к конкретным классам;  
- легко добавлять новый тип конференции без изменения существующего кода.

## 3. Диаграмма
```mermaid
classDiagram
    %% Клиентский класс
    class ConferenceApplication {
        - schedule_factory: IConferenceFactory
        +ConferenceApplication(factory: IConferenceFactory)
        +run()
    }

    %% Фабрики
    class OnlineConferenceFactory {
        - conference: Conference
        +create_schedule() ISchedule
        +create_notification_service() INotificationService
        +create_report_generator() IReportGenerator
    }

    class HybridConferenceFactory {
        - conference: Conference
        +create_schedule() ISchedule
        +create_notification_service() INotificationService
        +create_report_generator() IReportGenerator
    }

    %% Продукты
    class OnlineSchedule {
        - conference: Conference
        +show_schedule()
        +add_talk(talk: Talk)
        +remove_talk(talk_id: int)
    }

    class HybridSchedule {
        - conference: Conference
        +show_schedule()
        +add_talk(talk: Talk)
        +remove_talk(talk_id: int)
    }

    class EmailNotificationService {
        - conference: Conference
        +notify_all()
        +notify_participant(participant_id: str)
    }

    class MultiChannelNotificationService {
        - conference: Conference
        +notify_all()
        +notify_participant(participant_id: str)
    }

    class OnlineReportGenerator {
        +generate_summary_report()
        +generate_speaker_report(speaker_id: int)
    }

    class HybridReportGenerator {
        +generate_summary_report()
        +generate_speaker_report(speaker_id: int)
    }

    %% Ассоциации
    ConferenceApplication ..> OnlineConferenceFactory : uses
    ConferenceApplication ..> HybridConferenceFactory : uses

    OnlineConferenceFactory ..> OnlineSchedule : uses
    OnlineConferenceFactory ..> EmailNotificationService : uses
    OnlineConferenceFactory ..> OnlineReportGenerator : uses

    HybridConferenceFactory ..> HybridSchedule : uses
    HybridConferenceFactory ..> MultiChannelNotificationService : uses
    HybridConferenceFactory ..> HybridReportGenerator : uses
```

## 4. Реализация на C#

### 4.1 Абстрактные продукты

```csharp
public interface ISchedule
{
    void ShowSchedule();
    void AddTalk(Talk talk);
    void RemoveTalk(int talkId);
}

public interface INotificationService
{
    void NotifyAll();
    void NotifyParticipant(string participantId);
}

public interface IReportGenerator
{
    void GenerateSummaryReport();
    void GenerateSpeakerReport(int speakerId);
}
```

### 4.2 Абстрактная фабрика

```csharp
public interface IConferenceFactory
{
    ISchedule CreateSchedule();
    INotificationService CreateNotificationService();
    IReportGenerator CreateReportGenerator();
}
```

### 4.3 Конкретная фабрика: OnlineConferenceFactory

```csharp
public class OnlineConferenceFactory : IConferenceFactory
{
    private Conference _conference;
    public OnlineConferenceFactory(Conference conference) => _conference = conference;

    public ISchedule CreateSchedule() => new OnlineSchedule(_conference);
    public INotificationService CreateNotificationService() => new EmailNotificationService(_conference);
    public IReportGenerator CreateReportGenerator() => new OnlineReportGenerator();
}
```

### 4.4 Конкретные продукты онлайн-конференции

```csharp
public class OnlineSchedule : ISchedule
{
    private Conference _conference;

    public OnlineSchedule(Conference conference) => _conference = conference;

    public void ShowSchedule() { /* TODO: отобразить расписание онлайн */ }
    public void AddTalk(Talk talk) { /* TODO */ }
    public void RemoveTalk(int talkId) { /* TODO */ }
}

public class EmailNotificationService : INotificationService
{
    private Conference _conference;

    public EmailNotificationService(Conference conference) => _conference = conference;

    public void NotifyAll() { /* TODO: уведомления всем участникам */ }
    public void NotifyParticipant(string participantId) { /* TODO */ }
}

public class OnlineReportGenerator : IReportGenerator
{
    public void GenerateSummaryReport() { /* TODO */ }
    public void GenerateSpeakerReport(int speakerId) { /* TODO */ }
}
```

### 4.5 Конкретная фабрика: HybridConferenceFactory

```csharp
public class HybridConferenceFactory : IConferenceFactory
{
    private Conference _conference;
    public HybridConferenceFactory(Conference conference) => _conference = conference;

    public ISchedule CreateSchedule() => new HybridSchedule(_conference);
    public INotificationService CreateNotificationService() => new MultiChannelNotificationService(_conference);
    public IReportGenerator CreateReportGenerator() => new HybridReportGenerator();
}
```

### 4.6 Конкретные продукты гибридной конференции

```csharp
public class HybridSchedule : ISchedule
{
    private Conference _conference;

    public HybridSchedule(Conference conference) => _conference = conference;

    public void ShowSchedule() { /* TODO: показать оффлайн+онлайн расписание */ }
    public void AddTalk(Talk talk) { /* TODO */ }
    public void RemoveTalk(int talkId) { /* TODO */ }
}

public class MultiChannelNotificationService : INotificationService
{
    private Conference _conference;

    public MultiChannelNotificationService(Conference conference) => _conference = conference;

    public void NotifyAll() { /* TODO: Email+SMS уведомления */ }
    public void NotifyParticipant(string participantId) { /* TODO */ }
}

public class HybridReportGenerator : IReportGenerator
{
    public void GenerateSummaryReport() { /* TODO */ }
    public void GenerateSpeakerReport(int speakerId) { /* TODO */ }
}
```

---

### 4.7 Клиентский код

```csharp
public class ConferenceApplication
{
    private readonly ISchedule _schedule;
    private readonly INotificationService _notification;
    private readonly IReportGenerator _report;

    public ConferenceApplication(IConferenceFactory factory)
    {
        _schedule = factory.CreateSchedule();
        _notification = factory.CreateNotificationService();
        _report = factory.CreateReportGenerator();
    }

    public void Run()
    {
        _schedule.ShowSchedule();
        _notification.NotifyAll();
        _report.GenerateSummaryReport();
    }
}
```

## 5. Вывод
Шаблон Abstract Factory позволяет создавать согласованные семейства объектов,
изолируя клиентский код от конкретных реализаций и обеспечивая гибкость системы

## Factory Method (Фабричный метод)

## 1. Общее назначение шаблона

Шаблон **Factory Method** относится к порождающим шаблонам проектирования GoF.

Он определяет интерфейс для создания объекта, но позволяет подклассам решать, какой именно класс будет создан.

Отличие от Abstract Factory:
- Factory Method создаёт **один продукт**, а не семейство объектов;
- инкапсулирует логику выбора конкретной реализации;
- переносит ответственность за создание объекта в подклассы.

## 2. Назначение в системе управления конференциями

В системе управления конференциями требуется формировать расписание с учётом специфики формата проведения:
- Онлайн-конференция  
- Гибридная конференция  

При этом логика построения расписания отличается:
- онлайн-формат не учитывает залы;
- гибридный формат требует распределения докладов по аудиториям;
- возможны дополнительные проверки конфликтов времени;
- учитываются ограничения по вместимости залов.

Использование шаблона **Factory Method** позволяет:
- делегировать создание конкретного расписания подклассам;
- инкапсулировать различия в логике формирования;
- не изменять клиентский код при добавлении нового формата конференции.

## 3. Диаграмма
```mermaid
classDiagram
    %% Клиентский класс
    class ConferenceScheduler {
        - creator: ScheduleCreator
        +ConferenceScheduler(creator: ScheduleCreator)
        +run()
    }

    %% Абстрактный создатель
    class ScheduleCreator {
        <<abstract>>
        +create_schedule() ISchedule
    }

    %% Конкретные создатели
    class OnlineScheduleCreator {
        +create_schedule() OnlineSchedule
    }

    class HybridScheduleCreator {
        +create_schedule() HybridSchedule
    }

    %% Продукты
    class ISchedule {
        <<interface>>
        +build()
        +validate()
        +publish()
    }

    class OnlineSchedule {
        - conference: Conference
        +build()
        +validate()
        +publish()
    }

    class HybridSchedule {
        - conference: Conference
        +build()
        +validate()
        +publish()
    }

    %% Ассоциации
    ConferenceScheduler ..> ScheduleCreator : uses
    ScheduleCreator <|-- OnlineScheduleCreator
    ScheduleCreator <|-- HybridScheduleCreator

    OnlineScheduleCreator ..> OnlineSchedule : creates
    HybridScheduleCreator ..> HybridSchedule : creates

    OnlineSchedule ..|> ISchedule
    HybridSchedule ..|> ISchedule
```

## 4. Реализация на C#

### 4.1 Абстрактный продукт

```csharp
public interface ISchedule
{
    void Build();
    void Validate();
    void Publish();
}
```

### 4.2 Конкретные продукты

```csharp
public class OnlineSchedule : ISchedule
{
    private readonly Conference _conference;

    public OnlineSchedule(Conference conference)
    {
        _conference = conference;
    }

    public void Build()
    {
        // Формирование расписания без учета аудиторий
    }

    public void Validate()
    {
        // Проверка пересечений по времени
    }

    public void Publish()
    {
        // Публикация в веб-интерфейс
    }
}

public class HybridSchedule : ISchedule
{
    private readonly Conference _conference;

    public HybridSchedule(Conference conference)
    {
        _conference = conference;
    }

    public void Build()
    {
        // Распределение докладов по аудиториям
    }

    public void Validate()
    {
        // Проверка пересечений + проверка вместимости залов
    }

    public void Publish()
    {
        // Публикация в веб-интерфейс и вывод на экраны залов
    }
}
```

### 4.3 Абстрактный создатель (Creator)

```csharp
public abstract class ScheduleCreator
{
    protected Conference _conference;

    protected ScheduleCreator(Conference conference)
    {
        _conference = conference;
    }

    // Factory Method
    public abstract ISchedule CreateSchedule();

    // Бизнес-логика, использующая продукт
    public void GenerateAndPublish()
    {
        var schedule = CreateSchedule();

        schedule.Build();
        schedule.Validate();
        schedule.Publish();
    }
}
```

### 4.4 Конкретные создатели (Concrete Creators)

```csharp
public class OnlineScheduleCreator : ScheduleCreator
{
    public OnlineScheduleCreator(Conference conference)
        : base(conference) { }

    public override ISchedule CreateSchedule()
    {
        return new OnlineSchedule(_conference);
    }
}

public class HybridScheduleCreator : ScheduleCreator
{
    public HybridScheduleCreator(Conference conference)
        : base(conference) { }

    public override ISchedule CreateSchedule()
    {
        return new HybridSchedule(_conference);
    }
}
```

### 4.5 Клиентский код

```csharp
public class ConferenceScheduler
{
    private readonly ScheduleCreator _creator;

    public ConferenceScheduler(ScheduleCreator creator)
    {
        _creator = creator;
    }

    public void Run()
    {
        var schedule = _creator.CreateSchedule();
        schedule.Build();
        schedule.Validate();
        schedule.Publish();
    }
}
```

## 5. Вывод
Шаблон Factory Method позволил:
- инкапсулировать различия в логике построения расписания;
- делегировать создание конкретного продукта подклассам;
- изолировать клиентский код от конкретных реализаций;
- обеспечить расширяемость системы при добавлении новых форматов конференций.

## Prototype (Прототип)

## 1. Общее назначение шаблона

Шаблон **Prototype** относится к порождающим шаблонам проектирования GoF.

Он позволяет создавать новые объекты путём **клонирования уже существующих экземпляров**, вместо их создания через конструктор.

Основная идея шаблона:
> Объект сам отвечает за создание своей копии.

Шаблон применяется в случаях, когда:
- создание объекта является сложным или ресурсоёмким;
- объект содержит вложенные структуры;
- необходимо создавать множество похожих объектов с небольшими изменениями;
- требуется уменьшить зависимость кода от конкретных классов.

## 2. Назначение в системе управления конференциями

В системе управления конференциями может возникать необходимость:
- копирования расписания прошлой конференции для новой;
- создания альтернативных версий программы мероприятия;
- использования типового шаблона расписания.

Применение шаблона **Prototype** позволяет:
- клонировать уже настроенные объекты расписания;
- изменять только необходимые параметры (например, название конференции);
- избежать повторного ручного формирования структуры сессий;
- повысить гибкость системы.


## 3. UML-диаграмма
```mermaid
classDiagram
    %% Прототипы
    class ISchedulePrototype {
        <<interface>>
        +Clone() ISchedulePrototype
        +ShowSchedule()
    }

    class OnlineSchedule {
        +ConferenceName: string
        +Sessions: List~Session~
        +Clone() ISchedulePrototype
        +ShowSchedule()
    }

    class HybridSchedule {
        +ConferenceName: string
        +Venue: string
        +Sessions: List~Session~
        +Clone() ISchedulePrototype
        +ShowSchedule()
    }

    class Session {
        +Title: string
        +Speaker: string
        +StartTime: DateTime
        +Clone() Session
    }

    %% Ассоциации
    OnlineSchedule ..|> ISchedulePrototype
    HybridSchedule ..|> ISchedulePrototype
    OnlineSchedule "1" o-- "*" Session : contains
    HybridSchedule "1" o-- "*" Session : contains
```

## 4. Реализация на C#

### 4.1 Абстрактный прототип

```csharp
public interface ISchedulePrototype
{
    ISchedulePrototype Clone();
    void ShowSchedule();
}
```

## 4.2 Модель сессии

```csharp
// Модель отдельной сессии конференции.
public class Session
{
    public string Title { get; set; }
    public string Speaker { get; set; }
    public DateTime StartTime { get; set; }

    public Session(string title, string speaker, DateTime startTime)
    {
        Title = title;
        Speaker = speaker;
        StartTime = startTime;
    }

    // Глубокое копирование сессии.
    public Session Clone()
    {
        return new Session(Title, Speaker, StartTime);
    }
}
```

## 4.3 Конкретные прототипы

### OnlineSchedule

```csharp
// Прототип онлайн-расписания. Содержит список сессий и реализует глубокое копирование.
public class OnlineSchedule : ISchedulePrototype
{
    public string ConferenceName { get; set; }
    public List<Session> Sessions { get; set; } = new();

    public OnlineSchedule(string name)
    {
        ConferenceName = name;
    }

    // Реализация глубокого копирования.
    public ISchedulePrototype Clone()
    {
        var copy = (OnlineSchedule)this.MemberwiseClone();

        copy.Sessions = new List<Session>();

        foreach (var session in Sessions)
        {
            copy.Sessions.Add(session.Clone());
        }

        return copy;
    }

    public void ShowSchedule()
    {
        Console.WriteLine($"Online schedule for {ConferenceName}");

        foreach (var session in Sessions)
        {
            Console.WriteLine(
                $"{session.StartTime:t} | {session.Title} | {session.Speaker}");
        }
    }
}
```


### HybridSchedule

```csharp
// Прототип гибридного расписания. Дополнительно содержит информацию о площадке проведения.
public class HybridSchedule : ISchedulePrototype
{
    public string ConferenceName { get; set; }
    public string Venue { get; set; }

    public List<Session> Sessions { get; set; } = new();

    public HybridSchedule(string name, string venue)
    {
        ConferenceName = name;
        Venue = venue;
    }

    // Реализация глубокого копирования.
    public ISchedulePrototype Clone()
    {
        var copy = (HybridSchedule)this.MemberwiseClone();

        copy.Sessions = new List<Session>();

        foreach (var session in Sessions)
        {
            copy.Sessions.Add(session.Clone());
        }

        return copy;
    }

    public void ShowSchedule()
    {
        Console.WriteLine($"Hybrid schedule for {ConferenceName}");
        Console.WriteLine($"Venue: {Venue}");

        foreach (var session in Sessions)
        {
            Console.WriteLine(
                $"{session.StartTime:t} | {session.Title} | {session.Speaker}");
        }
    }
}
```

## 4.4 Клиентский код

```csharp
class Program
{
    static void Main()
    {
        // Создание исходного прототипа
        var original = new OnlineSchedule("TechConf 2026");

        original.Sessions.Add(new Session(
            "Cloud Architecture",
            "Ivan Petrov",
            new DateTime(2026, 5, 10, 10, 0, 0)));

        original.Sessions.Add(new Session(
            "Microservices",
            "Anna Smirnova",
            new DateTime(2026, 5, 10, 12, 0, 0)));

        // Клонирование объекта
        var copy = (OnlineSchedule)original.Clone();

        // Изменяем только название конференции
        copy.ConferenceName = "TechConf 2027";

        Console.WriteLine("=== ORIGINAL ===");
        original.ShowSchedule();

        Console.WriteLine();
        Console.WriteLine("=== COPY ===");
        copy.ShowSchedule();

        Console.ReadKey();
    }
}
```

## 5. Вывод
В системе управления конференциями данный шаблон:
- позволяет копировать сложные структуры расписаний;
- демонстрирует необходимость глубокого копирования вложенных объектов;
- упрощает создание новых конференций на основе существующих;
- снижает связанность клиентского кода с конкретными реализациями.

## Структурные шаблоны  
### Adapter (Адаптер)

