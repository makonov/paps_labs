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
![1-1-3](./PostmanScreens/1-1-3.png)

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

![1-1-1](./PostmanScreens/1-1-1.png)

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
![1-1-3](./PostmanScreens/1-1-3.png)


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

**Тест 2.1 — Успешное получение существующей конференции**  
**Строка запроса**: GET https://localhost:7123/api/v1/conferences/1

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
![2-1-2](./PostmanScreens/1-1-2.png)

**Тест 2.2 — Запрос несуществующей конференции (id = 9999)**  
**Строка запроса**: GET https://localhost:7123/api/v1/conferences/9999

Передаваемые параметры:
- Body: отсутствует
- Headers: отсутствуют
- Authorization: отсутствует (публичный эндпоинт)
- Params: нет (id передается в пути)

![2-2-1](./PostmanScreens/2-1-1.png)
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
![2-2-2](./PostmanScreens/1-1-2.png)
