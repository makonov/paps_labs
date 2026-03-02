# Лабораторная работа №6  
## Тема: Использование шаблонов проектирования  

## Порождающие шаблоны  
### Abstract Factory (Абстрактная фабрика)

## 1. Общее назначение шаблона

Шаблон **Abstract Factory** относится к порождающим шаблонам проектирования GoF.

Он предназначен для создания семейств взаимосвязанных или взаимозависимых объектов без указания их конкретных классов.

Шаблон позволяет:
- изолировать процесс создания объектов;
- обеспечить согласованность создаваемых объектов;
- заменить целое семейство объектов одной строкой кода;
- соблюдать принцип открытости/закрытости (Open/Closed Principle).

## 2. Назначение в системе управления конференциями

В системе управления конференциями существуют два основных типа конференций:
- Онлайн-конференция  
- Гибридная конференция  

Каждый тип конференции включает согласованный набор компонентов:
- Расписание  
- Сервис уведомлений  

Для разных типов конференций эти компоненты имеют разные реализации.

Использование шаблона **Abstract Factory** позволяет:
- создавать согласованный набор компонентов;
- не привязывать клиентский код к конкретным классам;
- легко добавлять новый тип конференции без изменения существующего кода.

---

## 3. UML-диаграмма
![1](./1.png)

## 4. Реализация на C#

### 4.1 Абстрактные продукты

```csharp
public interface ISchedule
{
    void ShowSchedule();
}

public interface INotificationService
{
    void Notify(string message);
}
```

---

### 4.2 Абстрактная фабрика

```csharp
public interface IConferenceFactory
{
    ISchedule CreateSchedule();
    INotificationService CreateNotificationService();
}
```

---

### 4.3 Конкретная фабрика: OnlineConferenceFactory

```csharp
public class OnlineConferenceFactory : IConferenceFactory
{
    public ISchedule CreateSchedule()
        => new OnlineSchedule();

    public INotificationService CreateNotificationService()
        => new EmailNotificationService();
}
```

---

### 4.4 Конкретные продукты онлайн-конференции

```csharp
public class OnlineSchedule : ISchedule
{
    public void ShowSchedule()
    {
        Console.WriteLine("Displaying online schedule.");
    }
}

public class EmailNotificationService : INotificationService
{
    public void Notify(string message)
    {
        Console.WriteLine($"Sending email: {message}");
    }
}
```

---

### 4.5 Конкретная фабрика: HybridConferenceFactory

```csharp
public class HybridConferenceFactory : IConferenceFactory
{
    public ISchedule CreateSchedule()
        => new HybridSchedule();

    public INotificationService CreateNotificationService()
        => new MultiChannelNotificationService();
}
```

---

### 4.6 Конкретные продукты гибридной конференции

```csharp
public class HybridSchedule : ISchedule
{
    public void ShowSchedule()
    {
        Console.WriteLine("Displaying hybrid schedule.");
    }
}

public class MultiChannelNotificationService : INotificationService
{
    public void Notify(string message)
    {
        Console.WriteLine($"Sending notification via Email + SMS: {message}");
    }
}
```

---

### 4.7 Клиентский код

```csharp
public class ConferenceApplication
{
    private readonly ISchedule _schedule;
    private readonly INotificationService _notification;

    public ConferenceApplication(IConferenceFactory factory)
    {
        _schedule = factory.CreateSchedule();
        _notification = factory.CreateNotificationService();
    }

    public void Run()
    {
        _schedule.ShowSchedule();
        _notification.Notify("Schedule updated!");
    }
}
```

---


## 5. Вывод

В ходе выполнения лабораторной работы был реализован порождающий шаблон проектирования **Abstract Factory**.

Шаблон позволил:

- создать семейства взаимосвязанных компонентов конференции;
- изолировать клиентский код от конкретных реализаций;
- обеспечить гибкость и расширяемость системы;
- добавить новый тип конференции без изменения существующего кода.

Таким образом, применение шаблона Abstract Factory повысило архитектурную гибкость системы управления конференциями.

## Factory Method (Фабричный метод)

## 1. Общее назначение шаблона

Шаблон **Factory Method** относится к порождающим шаблонам проектирования GoF.

Он позволяет создавать объекты, не указывая их конкретный класс, а делегируя создание подклассам.

Отличие от Abstract Factory:
- Factory Method создаёт **один продукт**, а не семейство продуктов.
- Клиент работает через абстракцию продукта, а конкретный класс выбирается в подклассе Creator.

## 2. Назначение в системе управления конференциями

В системе управления конференциями необходимо создавать расписание для разных типов конференций:
- Онлайн-конференция → `OnlineSchedule`  
- Гибридная конференция → `HybridSchedule`  

Шаблон **Factory Method** позволяет:
- клиенту работать только через интерфейс `ISchedule`;  
- добавлять новые типы расписания, создавая новый конкретный Creator;  
- не менять клиентский код при добавлении новых типов расписания.

## 3. UML-диаграмма
![2](./2.png)

## 4. Реализация на C#

### 4.1 Абстрактный продукт

```csharp
public interface ISchedule
{
    void ShowSchedule();
}
```

### 4.2 Конкретные продукты

```csharp
public class OnlineSchedule : ISchedule
{
    public void ShowSchedule()
    {
        Console.WriteLine("Displaying online schedule.");
    }
}

public class HybridSchedule : ISchedule
{
    public void ShowSchedule()
    {
        Console.WriteLine("Displaying hybrid schedule.");
    }
}
```

### 4.3 Абстрактный создатель (Creator)

```csharp
public abstract class ScheduleCreator
{
    // Фабричный метод
    public abstract ISchedule CreateSchedule();

    // Метод, использующий продукт
    public void Show()
    {
        var schedule = CreateSchedule();
        schedule.ShowSchedule();
    }
}
```

### 4.4 Конкретные создатели (Concrete Creators)

```csharp
public class OnlineScheduleCreator : ScheduleCreator
{
    public override ISchedule CreateSchedule()
    {
        return new OnlineSchedule();
    }
}

public class HybridScheduleCreator : ScheduleCreator
{
    public override ISchedule CreateSchedule()
    {
        return new HybridSchedule();
    }
}
```

### 4.5 Клиентский код

```csharp
class Program
{
    static void Main()
    {
        ScheduleCreator onlineCreator = new OnlineScheduleCreator();
        onlineCreator.Show();

        ScheduleCreator hybridCreator = new HybridScheduleCreator();
        hybridCreator.Show();

        Console.ReadKey();
    }
}
```

## 5. Вывод

## Prototype (Прототип)

## 1. Общее назначение шаблона

Шаблон **Prototype** относится к порождающим шаблонам GoF.

Он позволяет создавать новые объекты **копированием уже существующих**, вместо того чтобы создавать их с нуля через конструктор.

Особенности:
- Изолирует код от конкретных классов создаваемых объектов.  
- Ускоряет создание сложных объектов за счёт клонирования.  
- Позволяет динамически добавлять новые типы объектов без изменения существующего кода.

## 2. Назначение в системе управления конференциями

В системе управления конференциями необходимо быстро создавать **копии расписаний или уведомлений**, например:
- Копирование расписания прошлой конференции для новой конференции.  
- Создание шаблонов уведомлений для разных конференций.

Использование шаблона **Prototype** позволяет:
- клонировать существующие объекты без привязки к конкретным классам;  
- легко создавать новые экземпляры с теми же настройками;  
- уменьшить дублирование кода при создании схожих объектов.

## 3. UML-диаграмма
![3](./3.png)

## 4. Реализация на C#

### 4.1 Абстрактный прототип

```csharp
public interface ICloneableSchedule
{
    ICloneableSchedule Clone();
    void ShowSchedule();
}
```

### 4.2 Конкретные прототипы

```csharp
public class OnlineSchedule : ICloneableSchedule
{
    public string ConferenceName { get; set; }

    public OnlineSchedule(string name)
    {
        ConferenceName = name;
    }

    public ICloneableSchedule Clone()
    {
        // Простое клонирование (shallow copy)
        return (ICloneableSchedule)this.MemberwiseClone();
    }

    public void ShowSchedule()
    {
        Console.WriteLine($"Online schedule for {ConferenceName}");
    }
}

public class HybridSchedule : ICloneableSchedule
{
    public string ConferenceName { get; set; }

    public HybridSchedule(string name)
    {
        ConferenceName = name;
    }

    public ICloneableSchedule Clone()
    {
        return (ICloneableSchedule)this.MemberwiseClone();
    }

    public void ShowSchedule()
    {
        Console.WriteLine($"Hybrid schedule for {ConferenceName}");
    }
}
```

---

### 4.3 Клиентский код

```csharp
class Program
{
    static void Main()
    {
        ICloneableSchedule onlineOriginal = new OnlineSchedule("TechConf 2026");
        ICloneableSchedule onlineCopy = onlineOriginal.Clone();
        onlineCopy.ShowSchedule();

        ICloneableSchedule hybridOriginal = new HybridSchedule("GlobalConf 2026");
        ICloneableSchedule hybridCopy = hybridOriginal.Clone();
        hybridCopy.ShowSchedule();

        Console.ReadKey();
    }
}
```

## 5. Вывод

Шаблон **Prototype** позволяет:

- создавать копии объектов без знания их конкретного класса;  
- ускорять создание объектов, особенно сложных;  
- легко масштабировать систему при появлении новых типов расписаний или уведомлений.  

В лабораторной работе шаблон Prototype был реализован на примере клонирования расписаний онлайн и гибридных конференций.

Шаблон **Factory Method** позволяет:
- изолировать клиентский код от конкретных классов продукта;  
- легко расширять систему новым типом продукта, создавая новый конкретный Creator;  
- поддерживать принцип открытости/закрытости (Open/Closed Principle).  

В лабораторной работе шаблон Factory Method был реализован на примере создания расписаний конференций для онлайн и гибридных форматов.
