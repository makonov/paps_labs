# Лабораторная работа №4

## Работу выполнил: Чуприянов Макар РИС-22-1

Для выполнения лабораторной работы выбран **сервис управления конференциями** (Conference Management Service) — основной backend-сервис системы, который предоставляет REST API для работы с конференциями, докладами, спикерами, участниками, расписанием, голосованиями и материалами (слайдами).

Этот сервис отвечает за:
- создание, просмотр, редактирование и удаление конференций
- управление докладами внутри конференций (CRUD)
- голосование участников за/против докладов
- предоставление публичного расписания
- доступ к слайдам только авторизованным участникам
- базовую авторизацию и ролевую модель (организатор, спикер, участник)

Сервис реализован как монолитный REST API на ASP.NET Core (C#), с in-memory хранилищем данных для демонстрации и тестирования.

### Принятые проектные решения при проектировании API

При проектировании REST API для системы управления конференциями были приняты следующие решения:

1. **Именование ресурсов — существительные во множественном числе**  
   Все основные сущности представлены коллекциями: `/conferences`, `/talks`, `/speakers`, `/participants`, `/rooms`, `/votes`, `/slides`.  
   Это стандартный подход REST, чтобы URL сразу показывал, что мы работаем с набором объектов, а не с каким-то действием.

2. **Использование стандартных HTTP-методов по их прямому назначению**  
   - GET — получение данных (список, конкретный объект)  
   - POST — создание новой сущности  
   - PUT — полное обновление сущности (замена всех полей)  
   - DELETE — удаление  

3. **Вложенные ресурсы для логических связей**  
   Примеры путей:  
   - `/conferences/{conferenceId}/talks` — доклады конкретной конференции  
   - `/conferences/{conferenceId}/talks/{talkId}/votes` — голоса за конкретный доклад  
   - `/conferences/{conferenceId}/talks/{talkId}/slides` — слайды доклада  
   Это помогает сразу видеть иерархию и упрощает понимание структуры API.

4. **Версионирование API в пути**  
   Все эндпоинты начинаются с `/api/v1/...`  
   Это позволяет в будущем добавлять v2 без того, чтобы сломать существующие клиенты (мобильное приложение, сайт участников и т.д.).

5. **Единый формат данных — JSON**  
   Все запросы (кроме некоторых GET на файлы) и ответы используют `application/json`.  
   Исключение — сами слайды (PDF/PPTX), которые отдаются по прямой ссылке как файл.

6. **Возврат созданного/обновлённого объекта после POST и PUT**  
   После успешного создания (POST) или обновления (PUT) сервер возвращает полный актуальный объект со статусом 201 или 200.  
   Клиенту не приходится делать дополнительный GET, чтобы увидеть, что именно получилось (особенно важно для автогенерируемых полей: id, created_at, slug и т.д.).

7. **Единый формат ошибок**  
   При любой ошибке возвращается объект вида:  
   ```json
   {
     "error": "validation_failed",
     "message": "Поле 'start_time' должно быть позже текущего времени"
   }
   ```
   Плюс соответствующий HTTP-статус (400, 401, 403, 404, 409 и т.д.). Это сильно упрощает обработку ошибок на фронте.

8. **Авторизация и роли через JWT**  
   Все операции, кроме публичного чтения расписания (GET /conferences/{id}/schedule), требуют заголовка Authorization: Bearer <token>.
   Роли (organizer, speaker, participant) проверяются на бэкенде.
   Это обеспечивает безопасность + разные права (организатор может всё, спикер — только свои доклады, участник — голосовать и смотреть слайды).
   
9. **Публичное расписание без авторизации**
   Участники могут смотреть актуальное расписание и комнаты без входа в систему (GET /conferences/{id}/schedule, GET /conferences/{id}/talks/{talkId}).
   Это важно, чтобы любой человек мог зайти по ссылке и увидеть программу конференции.

10. **Ограничение доступа к слайдам только участникам конференции**
    Слайды отдаются только авторизованным участникам данной конференции (GET /conferences/{id}/talks/{talkId}/slides/{slideId}/file).
    Без подтверждённой регистрации на конференцию — 403 Forbidden.
    
11. **Поддержка пагинации и фильтров в списках**
    Для коллекций (доклады, голоса, участники) используются query-параметры: ?page=1&limit=20&sort=start_time&filter[speaker_id]=123
    Это необходимо, потому что на крупных конференциях может быть 200+ докладов.

## Тестирование API

Тестирование проводилось с помощью Postman.  
Для всех защищённых запросов сначала выполнялся POST /auth/login с нужной ролью, токен сохранялся в переменную окружения `jwt_token` и использовался в формате `Bearer {{jwt_token}}`.

### 1. POST /auth/login — Получение JWT-токена

**Тестируемое API**: /auth/login  
**Метод**: POST  

**Реализация**:
```C#
private readonly string _secret = "kJ9pL2mX8qW3rT5vY7zA0bC4dF6gH8jN1kP3mQ5sU7wX9yZ2!@ConferenceLab#2025";

[HttpPost("login")]
public IActionResult Login([FromBody] LoginRequest request)
{
    // Очень упрощённая проверка (в реале — БД + хэш)
    if (request.Username != "user" || request.Password != "pass")
        return Unauthorized(new { error = "invalid_credentials", message = "Неверный логин или пароль" });

    var claims = new[]
    {
        new Claim(ClaimTypes.Name, request.Username),
        new Claim(ClaimTypes.Role, request.Role)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddHours(1),
        signingCredentials: creds);

    return Ok(new JwtResponse { AccessToken = new JwtSecurityTokenHandler().WriteToken(token) });
}
```
**Формат передаваемых данных**:
```json
{
  "username": "string",
  "password": "string",
  "role": "string"
}
```

**Формат получаемых данных**:
```json
{
    "accessToken": "string"
}
```

**Тест 1.1 — Успешная авторизация (роль organizer)**  
**Строка запроса**: POST https://localhost:7212/auth/login

**Передаваемые данные (Body — raw JSON)**:
```json
{
  "username": "user",
  "password": "pass",
  "role": "organizer"
}
```
Заголовки и параметры:
- Headers: отсутствуют
- Authorization: отсутствует (публичный эндпоинт)
- Params: нет

![1-1-1](./PostmanScreens/1-1-1.png)
Полученный ответ:
Status: 200 OK
Body:
```json
{
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoidXNlciIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Im9yZ2FuaXplciIsImV4cCI6MTc2OTc5NjIwNn0._AcZkk2SuCTogU3GeXMPXXOuIvNuTLx1tRTeWK9nWXs"
}
```

Код автотестов:
```js
pm.test("Статус 200 OK", function () {
    pm.response.to.have.status(200);
});

pm.test("Есть accessToken", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property("accessToken");
    pm.environment.set("jwt_token", jsonData.accessToken);
});
```
![1-1-2](./PostmanScreens/1-1-2.png)

**Тест 1.2 — Ошибка авторизации (неверный пароль)**  
**Строка запроса**: POST https://localhost:7212/auth/login

**Передаваемые данные (Body — raw JSON)**:
```json
{
  "username": "user",
  "password": "wrongpass",
  "role": "organizer"
}
```
Заголовки и параметры:
- Headers: отсутствуют
- Authorization: отсутствует (публичный эндпоинт)
- Params: нет

![1-2-1](./PostmanScreens/1-2-1.png)

Полученный ответ:
Status: 401 Unauthorized
Body:
```json
{
    "error": "invalid_credentials",
    "message": "Неверный логин или пароль"
}
```

Код автотестов:
```js
pm.test("Статус 401", function () {
    pm.response.to.have.status(401);
});

pm.test("Ошибка invalid_credentials", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData.error).to.equal("invalid_credentials");
});
```
![1-2-3](./PostmanScreens/1-2-3.png)


### 2. GET /api/v1/conferences/{id} - Получение конкретной конференции
**Тестируемое API**: /api/v1/conferences/{id}
**Метод**: GET  

**Реализация**:
```C#
[HttpGet("{id}")]
[AllowAnonymous]
public ActionResult<Conference> Get(int id)
{
    if (!InMemoryStore.Conferences.TryGetValue(id, out var conf))
        return NotFound(new ErrorResponse { Error = "not_found", Message = "Конференция не найдена" });

    return Ok(conf);
}
```

**Формат получаемых данных**:
```json
{
  "id" : "int",
  "name": "string",
  "startDate": "string" (формат YYYY-MM-DD),
  "endDate": "string" (формат YYYY-MM-DD),
  "location": "string" 
}
```

**Тест 2.1 — Успешное получение существующей конференции**  
**Строка запроса**: GET https://localhost:7212/api/v1/conferences/1

Передаваемые параметры:
- Body: отсутствует
- Headers: отсутствуют
- Authorization: отсутствует (публичный эндпоинт)
- Params: нет (id передается в пути)

![2-1-1](./PostmanScreens/2-1-1.png)
Полученный ответ:
Status: 200 OK
Body:
```json
{
    "id": 1,
    "name": "DevConf Spring 2026",
    "startDate": "2026-04-15",
    "endDate": "2026-04-17",
    "location": "Санкт-Петербург"
}
```

Код автотестов:
```js
pm.test("Статус ответа 200 OK", function () {
    pm.response.to.have.status(200);
});

pm.test("В ответе есть поле id и оно равно 1", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property("id");
    pm.expect(jsonData.id).to.equal(1);
});

pm.test("В ответе есть название конференции", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property("name");
    pm.expect(jsonData.name).to.be.a("string");
    pm.expect(jsonData.name).to.not.be.empty;
});

pm.test("Объект конференции имеет ожидаемую структуру и точные значения", function () {
    var jsonData = pm.response.json();

    pm.expect(jsonData).to.have.all.keys("id", "name", "startDate", "endDate", "location");

    pm.expect(jsonData.id).to.be.a("number");
    pm.expect(jsonData.name).to.be.a("string");
    pm.expect(jsonData.startDate).to.be.a("string");
    pm.expect(jsonData.endDate).to.be.a("string");
    pm.expect(jsonData.location).to.be.a("string");

    pm.expect(jsonData.id).to.equal(1);
    pm.expect(jsonData.name).to.equal("DevConf Spring 2026");
    pm.expect(jsonData.startDate).to.equal("2026-04-15");
    pm.expect(jsonData.endDate).to.equal("2026-04-17");
    pm.expect(jsonData.location).to.equal("Санкт-Петербург");
});

pm.test("В объекте ровно 5 полей (нет лишних)", function () {
    var jsonData = pm.response.json();
    pm.expect(Object.keys(jsonData).length).to.equal(5);
});
```
![2-1-2](./PostmanScreens/2-1-2.png)

**Тест 2.2 — Запрос несуществующей конференции (id = 9999)**  
**Строка запроса**: GET https://localhost:7212/api/v1/conferences/9999

Передаваемые параметры:
- Body: отсутствует
- Headers: отсутствуют
- Authorization: отсутствует (публичный эндпоинт)
- Params: нет (id передается в пути)

![2-2-1](./PostmanScreens/2-2-1.png)
Полученный ответ:
Status: 400 Not Found
Body:
```json
{
  "error": "not_found",
  "message": "Конференция не найдена"
}
```

Код автотестов:
```js
pm.test("Статус ответа 404 Not Found", function () {
    pm.response.to.have.status(404);
});

pm.test("В ответе правильный тип ошибки", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property("error");
    pm.expect(jsonData.error).to.equal("not_found");
});

pm.test("Есть сообщение об ошибке", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property("message");
    pm.expect(jsonData.message).to.include("Конференция не найдена");
});
```
![2-2-2](./PostmanScreens/2-2-2.png)

### 3. POST /api/v1/conferences — Создание конференции

**Тестируемое API**: /api/v1/conferences
**Метод**: POST  

**Реализация**:
```C#
[HttpPost]
[Authorize(Roles = "organizer")]
public ActionResult<Conference> Create([FromBody] Conference conf)
{
    // 1. Проверка, что тело пришло
    if (conf == null)
    {
        return BadRequest(new ErrorResponse
        {
            Error = "invalid_request",
            Message = "Тело запроса отсутствует или некорректно"
        });
    }

    // 2. Проверка обязательных полей
    if (string.IsNullOrWhiteSpace(conf.Name))
    {
        return BadRequest(new ErrorResponse
        {
            Error = "validation_error",
            Message = "Название конференции обязательно"
        });
    }

    if (string.IsNullOrWhiteSpace(conf.StartDate))
    {
        return BadRequest(new ErrorResponse
        {
            Error = "validation_error",
            Message = "Дата начала конференции обязательна"
        });
    }

    if (string.IsNullOrWhiteSpace(conf.EndDate))
    {
        return BadRequest(new ErrorResponse
        {
            Error = "validation_error",
            Message = "Дата окончания конференции обязательна"
        });
    }

    if (string.IsNullOrWhiteSpace(conf.Location))
    {
        return BadRequest(new ErrorResponse
        {
            Error = "validation_error",
            Message = "Место проведения конференции обязательно"
        });
    }

    // 3. Проверка логики дат (startDate < endDate)
    if (!DateTime.TryParse(conf.StartDate, out var start) ||
        !DateTime.TryParse(conf.EndDate, out var end))
    {
        return BadRequest(new ErrorResponse
        {
            Error = "validation_error",
            Message = "Некорректный формат дат (ожидается YYYY-MM-DD)"
        });
    }

    if (start >= end)
    {
        return BadRequest(new ErrorResponse
        {
            Error = "validation_error",
            Message = "Дата начала должна быть раньше даты окончания"
        });
    }

    conf.Id = InMemoryStore.NextConferenceId();
    InMemoryStore.Conferences[conf.Id] = conf;

    return CreatedAtAction(nameof(Get), new { id = conf.Id }, conf);
}
```
**Формат передаваемых данных**:
```json
{           
  "name": "string",
  "startDate": "string" (формат YYYY-MM-DD),
  "endDate": "string" (формат YYYY-MM-DD),
  "location": "string"
}
```

**Формат получаемых данных**:
```json
{
  "id": "int",               
  "name": "string",
  "startDate": "string" (формат YYYY-MM-DD),
  "endDate": "string" (формат YYYY-MM-DD),
  "location": "string"
}
```

**Тест 3.1 — Успешное создание конференции**  
**Строка запроса**: POST https://localhost:7212/api/v1/conferences

**Передаваемые данные (Body — raw JSON)**:
```json
{
  "name": "Конференция Осень 2026",
  "startDate": "2026-09-10",
  "endDate": "2026-09-12",
  "location": "Екатеринбург"
}
```
Заголовки и параметры:
- Headers: authorization - Bearer {{jwt_token}}  (токен роли organizer)
- Params: нет

![3-1-1](./PostmanScreens/3-1-1.png)
Полученный ответ:
Status: 201 Created
Body:
```json
{
    "id": 4,
    "name": "Конференция Осень 2026",
    "startDate": "2026-09-10",
    "endDate": "2026-09-12",
    "location": "Екатеринбург"
}
```

Код автотестов:
```js
pm.test("Статус ответа 201 Created", function () {
    pm.response.to.have.status(201);
});

pm.test("В ответе есть сгенерированный id", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property("id");
    pm.expect(jsonData.id).to.be.a("number").and.be.above(0);
});

pm.test("Возвращённые данные совпадают с отправленными", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData.name).to.equal("Конференция Осень 2026");
    pm.expect(jsonData.startDate).to.equal("2026-09-10");
    pm.expect(jsonData.endDate).to.equal("2026-09-12");
    pm.expect(jsonData.location).to.equal("Екатеринбург");
});

```
![3-1-2](./PostmanScreens/3-1-2.png)
![3-1-3](./PostmanScreens/3-1-3.png)

**Тест 3.2 — Ошибка валидации (отсутствует обязательное поле "name")**  
**Строка запроса**: POST https://localhost:7212/api/v1/conferences

**Передаваемые данные (Body — raw JSON)**:
```json
{
  "startDate": "2026-09-15",
  "endDate": "2026-09-17",
  "location": "Новосибирск"
}
```
Заголовки и параметры:
- Headers: authorization - Bearer {{jwt_token}}  (токен роли organizer)
- Params: нет

![3-2-1](./PostmanScreens/3-2-1.png)
Полученный ответ:
Status: 400 Bad Request
Body:
```json
{
  "error": "validation_error",
  "message": "Название конференции обязательно"
}
```

Код автотестов:
```js
pm.test("Статус ответа 400 Bad Request", function () {
    pm.response.to.have.status(400);
});

pm.test("Ошибка из-за отсутствия названия", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property("error");
    pm.expect(jsonData.error).to.equal("validation_error");
    pm.expect(jsonData).to.have.property("message");
    pm.expect(jsonData.message).to.include("Название конференции обязательно");
});

pm.test("Нет поля id в ответе об ошибке", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.not.have.property("id");
});
```
![3-2-2](./PostmanScreens/3-2-2.png)
![3-2-3](./PostmanScreens/3-2-3.png)

**Тест 3.3 — Попытка создания конференции без токена авторизации**  
**Строка запроса**: POST https://localhost:7212/api/v1/conferences

**Передаваемые данные (Body — raw JSON)**:
```json
{
  "name": "Конференция без токена",
  "startDate": "2026-10-01",
  "endDate": "2026-10-03",
  "location": "Казань"
}
```
Заголовки и параметры:
- Headers: нет
- Params: нет
- Authorization: нет

![3-3-1](./PostmanScreens/3-3-1.png)
Полученный ответ:
- Status: 401 Unauthorized
- Body: нет

Код автотестов:
```js
pm.test("Статус ответа 401 Unauthorized — нет токена", function () {
    pm.response.to.have.status(401);
});

pm.test("Есть заголовок WWW-Authenticate с Bearer", function () {
    var wwwAuth = pm.response.headers.get("WWW-Authenticate");
    pm.expect(wwwAuth).to.exist;
    pm.expect(wwwAuth).to.include("Bearer");
});
```
![3-3-2](./PostmanScreens/3-3-2.png)

### 4. GET /api/v1/conferences/{conferenceId}/schedule — Расписание конференции
**Тестируемое API**: /api/v1/conferences/{conferenceId}/schedule
**Метод**: GET  

**Реализация**:
```C#
[ApiController]
[Route("api/v1/conferences/{conferenceId}/schedule")]
public class ScheduleController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]   // публичный доступ — любой может смотреть расписание
    public ActionResult<List<Talk>> GetSchedule(int conferenceId)
    {
        if (!InMemoryStore.Conferences.ContainsKey(conferenceId))
            return NotFound(new ErrorResponse { Error = "not_found", Message = "Конференция не найдена" });

        var schedule = InMemoryStore.Talks.Values
            .Where(t => t.ConferenceId == conferenceId)
            .OrderBy(t => t.StartTime)
            .ToList();

        return Ok(schedule);
    }
}
```

**Формат получаемых данных** - массив объектов Talk:
```json
[
   {
     "id": "int",
     "conferenceId": "int",
     "title": "string",
     "speakerId": "int",
     "startTime": "string" (формат YYYY-MM-DD),
     "room": "string",
     "slides": "string" или "null"
   },
...
]
```

**Тест 4.1 — Успешное получение расписания**  
**Строка запроса**: GET https://localhost:7212/api/v1/conferences/{{conference_id}}/schedule

Передаваемые параметры:
- Body: отсутствует
- Headers: отсутствуют
- Authorization: отсутствует (публичный эндпоинт)
- Params: нет (id передается в пути)

![4-1-1](./PostmanScreens/4-1-1.png)
Полученный ответ:
Status: 200 OK
Body:
```json
[
    {
        "id": 1,
        "conferenceId": 1,
        "title": "JWT авторизация в ASP.NET Core",
        "speakerId": 1,
        "startTime": "2026-04-15T11:00",
        "room": "Зал 2",
        "slides": "https://slides.com/jwt-asp"
    },
    {
        "id": 2,
        "conferenceId": 1,
        "title": "Тестирование REST API с Postman",
        "speakerId": 1,
        "startTime": "2026-04-15T14:00",
        "room": "Зал 3",
        "slides": "https://example.com/postman-testing.pdf"
    }
]
```

Код автотестов:
```js
pm.test("Статус ответа 200 OK", function () {
    pm.response.to.have.status(200);
});

pm.test("Ответ — массив объектов докладов", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.be.an("array");
});

pm.test("В массиве минимум 2 доклада", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData.length).to.be.at.least(2);
});

pm.test("Каждый доклад имеет все ожидаемые поля", function () {
    var jsonData = pm.response.json();
    
    jsonData.forEach(function(talk, index) {
        pm.expect(talk).to.have.all.keys(
            "id", 
            "conferenceId", 
            "title", 
            "speakerId", 
            "startTime", 
            "room", 
            "slides"
        );
        
        pm.expect(talk.id).to.be.a("number");
        pm.expect(talk.conferenceId).to.be.a("number");
        pm.expect(talk.title).to.be.a("string").and.not.empty;
        pm.expect(talk.speakerId).to.be.a("number");
        pm.expect(talk.startTime).to.be.a("string").and.match(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}$/);
        pm.expect(talk.room).to.be.a("string");
        pm.expect(talk.slides).to.be.a("string"); // или null — но в твоём примере строка
    });
});

pm.test("Первый доклад имеет ожидаемые значения", function () {
    var jsonData = pm.response.json();
    var firstTalk = jsonData[0];
    
    pm.expect(firstTalk.id).to.equal(1);
    pm.expect(firstTalk.conferenceId).to.equal(1);
    pm.expect(firstTalk.title).to.equal("JWT авторизация в ASP.NET Core");
    pm.expect(firstTalk.speakerId).to.equal(1);
    pm.expect(firstTalk.startTime).to.equal("2026-04-15T11:00");
    pm.expect(firstTalk.room).to.equal("Зал 2");
    pm.expect(firstTalk.slides).to.equal("https://slides.com/jwt-asp");
});
```
![4-1-2](./PostmanScreens/4-1-2.png)

**Тест 4.2 — Запрос расписания по несуществующей конференции (id = 9999)**  
**Строка запроса**: GET https://localhost:7212/api/v1/conferences/9999/schedule

Передаваемые параметры:
- Body: отсутствует
- Headers: отсутствуют
- Authorization: отсутствует (публичный эндпоинт)
- Params: нет (id передается в пути)

![4-2-1](./PostmanScreens/4-2-1.png)
Полученный ответ:
Status: 400 Not Found
Body:
```json
{
  "error": "not_found",
  "message": "Конференция не найдена"
}
```

Код автотестов:
```js
pm.test("Статус ответа 404 Not Found", function () {
    pm.response.to.have.status(404);
});

pm.test("В ответе правильный тип ошибки", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property("error");
    pm.expect(jsonData.error).to.equal("not_found");
});

pm.test("Есть сообщение об ошибке", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property("message");
    pm.expect(jsonData.message).to.include("Конференция не найдена");
});
```
![4-2-2](./PostmanScreens/4-2-2.png)

### 5. POST /api/v1/conferences/{conferenceId}/talks — Создание доклада

**Тестируемое API**: /api/v1/conferences/{conferenceId}/talks
**Метод**: POST  

**Реализация**:
```C#
[HttpPost]
[Authorize(Roles = "speaker")]
public ActionResult<Talk> Create(int conferenceId, [FromBody] Talk talk)
{
    // 1. Проверка, что тело запроса пришло
    if (talk == null)
    {
        return BadRequest(new ErrorResponse
        {
            Error = "invalid_request",
            Message = "Тело запроса отсутствует или некорректно"
        });
    }

    // 2. Проверка обязательных полей
    if (string.IsNullOrWhiteSpace(talk.Title))
    {
        return BadRequest(new ErrorResponse
        {
            Error = "validation_error",
            Message = "Название доклада обязательно"
        });
    }

    if (talk.SpeakerId <= 0)
    {
        return BadRequest(new ErrorResponse
        {
            Error = "validation_error",
            Message = "ID спикера обязателен и должен быть положительным числом"
        });
    }

    if (string.IsNullOrWhiteSpace(talk.StartTime))
    {
        return BadRequest(new ErrorResponse
        {
            Error = "validation_error",
            Message = "Время начала доклада обязательно"
        });
    }

    if (string.IsNullOrWhiteSpace(talk.Room))
    {
        return BadRequest(new ErrorResponse
        {
            Error = "validation_error",
            Message = "Аудитория (комната) обязательна"
        });
    }

    // 3. Проверка, существует ли конференция
    if (!InMemoryStore.Conferences.ContainsKey(conferenceId))
    {
        return NotFound(new ErrorResponse
        {
            Error = "not_found",
            Message = "Конференция не найдена"
        });
    }

    // 4. Проверка, существует ли спикер
    if (!InMemoryStore.Speakers.ContainsKey(talk.SpeakerId))
    {
        return NotFound(new ErrorResponse
        {
            Error = "not_found",
            Message = "Спикер с указанным ID не найден"
        });
    }

    // 5. Проверка формата времени
    if (!DateTime.TryParse(talk.StartTime, out var startTime))
    {
        return BadRequest(new ErrorResponse
        {
            Error = "validation_error",
            Message = "Некорректный формат времени начала (ожидается YYYY-MM-DDTHH:MM)"
        });
    }

    talk.Id = InMemoryStore.NextTalkId();
    talk.ConferenceId = conferenceId;
    InMemoryStore.Talks[talk.Id] = talk;

    return CreatedAtAction(nameof(GetTalk), new { conferenceId, talkId = talk.Id }, talk);
}
```
**Формат передаваемых данных**:
```json
{
  "title": "string",
  "speakerId": "int",
  "startTime": "string" (формат YYYY-MM-DDTHH:MM),
  "room": "string",
  "slides": "string" или "null"
}
```

**Формат получаемых данных**:
```json
{
  "id": "int",
  "conferenceId": "int",
  "title": "string",
  "speakerId": "int",
  "startTime": "string" (формат YYYY-MM-DDTHH:MM),
  "room": "string",
  "slides": "string" или "null"
}
```

**Тест 5.1 — Успешное создание доклада**  
**Строка запроса**: POST https://localhost:7212/api/v1/conferences/1/talks

**Передаваемые данные (Body — raw JSON)**:
```json
{
  "title": "Современные паттерны в .NET",
  "speakerId": 1,
  "startTime": "2026-04-15T15:30",
  "room": "Зал В",
  "slides": "https://slides.com/dotnet-patterns"
}
```
Заголовки и параметры:
- Headers: authorization - Bearer {{jwt_token}}  (токен роли speaker)
- Params: нет (conferenceId в пути)

![5-1-1](./PostmanScreens/5-1-1.png)
Полученный ответ:
Status: 201 Created
Body:
```json
{
    "id": 3,
    "conferenceId": 1,
    "title": "Современные паттерны в .NET",
    "speakerId": 1,
    "startTime": "2026-04-15T15:30",
    "room": "Зал В",
    "slides": "https://slides.com/dotnet-patterns"
}
```

Код автотестов:
```js
pm.test("Статус ответа 400 Bad Request", function () {
    pm.response.to.have.status(400);
});

pm.test("Ошибка из-за отсутствия названия доклада", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property("Error");
    pm.expect(jsonData.Error).to.equal("validation_error");
    pm.expect(jsonData).to.have.property("Message");
    pm.expect(jsonData.Message).to.include("Название доклада обязательно");
});

pm.test("Нет поля id в ответе об ошибке", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.not.have.property("id");
});
```
![5-1-2](./PostmanScreens/5-1-2.png)
![5-1-3](./PostmanScreens/5-1-3.png)

**Тест 5.2 — Ошибка валидации при создании доклада (отсутствует обязательное поле title)**  
**Строка запроса**: POST https://localhost:7212/api/v1/conferences/1/talks

**Передаваемые данные (Body — raw JSON)**:
```json
{
  "speakerId": 1,
  "startTime": "2026-04-15T16:00",
  "room": "Зал Г",
  "slides": "https://example.com/slides.pdf"
}
```
Заголовки и параметры:
- Headers: authorization - Bearer {{jwt_token}}  (токен роли speaker)
- Params: нет (conferenceId в пути)

![5-2-1](./PostmanScreens/5-2-1.png)
Полученный ответ:
Status: 400 Bad Request
Body:
```json
{
    "error": "validation_error",
    "message": "Название доклада обязательно"
}
```

Код автотестов:
```js
pm.test("Статус ответа 400 Bad Request", function () {
    pm.response.to.have.status(400);
});

pm.test("Ошибка из-за отсутствия названия доклада", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property("error");
    pm.expect(jsonData.error).to.equal("validation_error");
    pm.expect(jsonData).to.have.property("message");
    pm.expect(jsonData.message).to.include("Название доклада обязательно");
});

pm.test("Нет поля id в ответе об ошибке", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.not.have.property("id");
});
```
![5-2-2](./PostmanScreens/5-1-2.png)
![5-2-3](./PostmanScreens/5-1-3.png)

### 6. POST /api/v1/conferences/{conferenceId}/talks/{talkId}/votes — Голосование

**Тестируемое API**: /api/v1/conferences/{conferenceId}/talks/{talkId}/votes
**Метод**: POST  

**Реализация**:
```C#
[HttpPost]
[Authorize(Roles = "participant")]
public ActionResult<Vote> CreateVote(int conferenceId, int talkId, [FromBody] Vote vote)
{
    // 1. Проверка, что тело запроса пришло
    if (vote == null)
    {
        return BadRequest(new ErrorResponse
        {
            Error = "invalid_request",
            Message = "Тело запроса отсутствует или некорректно"
        });
    }

    // 2. Проверка обязательных полей
    if (vote.ParticipantId <= 0)
    {
        return BadRequest(new ErrorResponse
        {
            Error = "validation_error",
            Message = "ID участника обязателен и должен быть положительным числом"
        });
    }

    // 3. Проверка существования конференции и доклада
    if (!InMemoryStore.Conferences.ContainsKey(conferenceId))
        return NotFound(new ErrorResponse { Error = "not_found", Message = "Конференция не найдена" });

    if (!InMemoryStore.Talks.TryGetValue(talkId, out var talk) || talk.ConferenceId != conferenceId)
        return NotFound(new ErrorResponse { Error = "not_found", Message = "Доклад не найден" });

    // один участник голосует один раз за доклад
    var existingVote = InMemoryStore.Votes.Values
        .FirstOrDefault(v => v.TalkId == talkId && v.ParticipantId == vote.ParticipantId);

    if (existingVote != null)
    {
        return Conflict(new ErrorResponse
        {
            Error = "already_voted",
            Message = "Участник уже проголосовал за этот доклад"
        });
    }

    vote.Id = InMemoryStore.NextVoteId();
    vote.TalkId = talkId;
    InMemoryStore.Votes[vote.Id] = vote;

    return CreatedAtAction(nameof(GetVotes), new { conferenceId, talkId }, vote);
}
```

**Формат передаваемых данных**:
```json
{
  "participantId": "int"  > 0,
  "value": "bool" 
}
```

**Формат получаемых данных**:
```json
{
  "id": "int",
  "talkId": "int",
  "participantId": "int",
  "value": "bool"
}
```

**Тест 6.1 — Успешное создание голоса**  
**Строка запроса**: POST https://localhost:7212/api/v1/conferences/1/talks/1/votes

**Передаваемые данные (Body — raw JSON)**:
```json
{
  "participantId": 1004,
  "value": true
}
```
Заголовки и параметры:
- Headers: authorization - Bearer {{jwt_token}}  (токен роли speaker)
- Params: нет (conferenceId и talkId в пути)

![6-1-1](./PostmanScreens/6-1-1.png)
Полученный ответ:
Status: 201 Created
Body:
```json
{
    "id": 4,
    "talkId": 1,
    "participantId": 1004,
    "value": true
}
```

Код автотестов:
```js
pm.test("Статус ответа 201 Created", function () {
    pm.response.to.have.status(201);
});

pm.test("В ответе есть сгенерированный id", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property("id");
    pm.expect(jsonData.id).to.be.a("number").and.be.above(0);
});

pm.test("Голос положительный и participantId совпадает", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData.value).to.be.true;
    pm.expect(jsonData.participantId).to.equal(1004);
});

pm.test("talkId в ответе совпадает с переданным", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData.talkId).to.equal(1);
});
```
![6-1-2](./PostmanScreens/6-1-2.png)
![6-1-3](./PostmanScreens/6-1-3.png)

**Тест 6.2 — Ошибка при повторном голосовании (один участник голосует второй раз)**  
**Строка запроса**: POST https://localhost:7212/api/v1/conferences/1/talks/1/votes (тот же conferenceId и talkId, что в первом тесте)

**Передаваемые данные (Body — raw JSON)**:
```json
{
  "participantId": 1004,
  "value": true
}
```
Заголовки и параметры:
- Headers: authorization - Bearer {{jwt_token}}  (токен роли speaker)
- Params: нет (conferenceId и talkId в пути)

![6-2-1](./PostmanScreens/6-2-1.png)
Полученный ответ:
Status: 201 Created
Body:
```json
{
    "error": "already_voted",
    "message": "Участник уже проголосовал за этот доклад"
}
```

Код автотестов:
```js
pm.test("Статус ответа 409 Conflict при повторном голосовании", function () {
    pm.response.to.have.status(409);
});

pm.test("Ошибка уже проголосовал", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property("error");
    pm.expect(jsonData.error).to.equal("already_voted");
    pm.expect(jsonData).to.have.property("message");
    pm.expect(jsonData.message).to.include("уже проголосовал");
});

pm.test("Нет поля id в ответе об ошибке", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.not.have.property("id");
});
```
![6-2-2](./PostmanScreens/6-2-2.png)
![6-2-3](./PostmanScreens/6-2-3.png)
