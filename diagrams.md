# Диаграммы GymManager

Диаграммы описаны на mermaid — GitHub и GitLab рендерят их прямо в вебе,
а исходник остаётся текстовым и правится вместе с кодом.

---

## 1. Модель данных

Шесть таблиц. Обрати внимание на два решения:

- у `TICKETS` **нет колонки status** — он вычисляется представлением
  `v_tickets`, потому что зависит от текущей даты;
- `VISITS` связана с клиентом только через `TICKETS`, прямой ссылки нет —
  иначе возможен рассинхрон, когда посещение указывает на одного клиента,
  а абонемент принадлежит другому.

```mermaid
erDiagram
  CLIENTS ||--o{ TICKETS : "оформляет"
  TICKET_TYPES ||--o{ TICKETS : "задаёт условия"
  TICKETS ||--o{ VISITS : "содержит"
  TRAINERS ||--o{ VISITS : "проводит"
  USERS ||--o{ VISITS : "фиксирует"

  CLIENTS {
    uuid id PK
    text last_name
    text first_name
    text middle_name
    varchar phone
    text email
    text status
    timestamptz created_at
    timestamptz updated_at
  }
  TICKET_TYPES {
    uuid id PK
    text code UK
    text name
    int duration_days
    int default_visits
    bool is_active
  }
  TICKETS {
    uuid id PK
    uuid client_id FK
    uuid ticket_type_id FK
    date date_start
    date date_end
    int visits_limit
    int visits_used
    bool is_cancelled
  }
  VISITS {
    uuid id PK
    uuid ticket_id FK
    timestamptz visited_at
    uuid trainer_id FK
    uuid user_id FK
  }
  TRAINERS {
    uuid id PK
    text last_name
    text first_name
    text middle_name
    bool is_active
  }
  USERS {
    uuid id PK
    text username UK
    text password_hash
    text last_name
    text first_name
    bool is_active
  }
```

### Представление v_tickets

Статус — не свойство записи, а функция от записи и текущего момента.
Хранимое поле неизбежно устаревает: абонемент истекает в полночь,
но операции записи при этом не происходит.

```mermaid
flowchart TD
  A["Абонемент из tickets"] --> B{"is_cancelled?"}
  B -- да --> C["cancelled"]
  B -- нет --> D{"CURRENT_DATE < date_start?"}
  D -- да --> E["pending"]
  D -- нет --> F{"CURRENT_DATE > date_end?"}
  F -- да --> G["expired"]
  F -- нет --> H{"visits_used >= visits_limit?"}
  H -- да --> I["exhausted"]
  H -- нет --> J["active"]
```

Порядок проверок задаёт приоритет причин: отменённый абонемент остаётся
отменённым, даже если у него ещё и срок вышел.

---

## 2. Архитектура слоёв

```mermaid
flowchart LR
  API["GymManager.Api<br/>HTTP, контроллеры"]
  APP["GymManager.Application<br/>правила, интерфейсы"]
  INF["GymManager.Infrastructure<br/>EF Core, репозитории"]

  API -->|ссылается| APP
  INF -->|ссылается| APP
  API -.->|только регистрация в DI| INF
```

Обе сплошные стрелки сходятся в `Application`, и она не зависит ни от кого.
Интерфейсы репозиториев объявлены в ней, а реализованы в `Infrastructure` —
интерфейс принадлежит тому, кто им пользуется, а не тому, кто его реализует.

Пунктирная связь — единственное исключение: `Program.cs` знает обе стороны,
чтобы их соединить. Это место называют Composition Root.

---

## 3. Фиксация посещения

Самая сложная операция: две записи в одной транзакции, проверка правил
между ними и защита от гонки.

```mermaid
sequenceDiagram
  participant F as Фронтенд
  participant C as VisitsController
  participant S as VisitService
  participant U as UnitOfWork
  participant R as TicketRepository
  participant D as PostgreSQL

  F->>C: POST /api/visits {ticketId}
  C->>S: RegisterAsync(command, userId из токена)
  S->>U: ExecuteInTransactionAsync
  U->>D: BEGIN
  S->>R: GetForUpdateAsync(ticketId)
  R->>D: SELECT ... FOR UPDATE
  D-->>R: строка абонемента заблокирована
  R-->>S: TicketState

  alt правило нарушено
    S->>U: BusinessRuleException
    U->>D: ROLLBACK
    C-->>F: 409 + code причины
  else проверки пройдены
    S->>R: AddVisitAsync
    R->>D: INSERT visits
    R->>D: UPDATE tickets visits_used
    U->>D: COMMIT
    C-->>F: 200 + остаток посещений
  end
```

**Почему FOR UPDATE.** Без блокировки два одновременных запроса по абонементу
с одним оставшимся посещением оба прошли бы проверку и списали два:

| Шаг | Запрос A | Запрос B |
|---|---|---|
| 1 | читает: использовано 7 из 8 | |
| 2 | | читает: использовано 7 из 8 |
| 3 | проверка пройдена | проверка пройдена |
| 4 | пишет 8 | |
| 5 | | пишет 8 |

Блокировка превращает шаг 2 в ожидание: запрос B прочитает уже 8
и честно откажет.

Последний рубеж — `CHECK (visits_used <= visits_limit)` на уровне БД.

---

## 4. Список клиентов с поиском

```mermaid
sequenceDiagram
  actor A as Администратор
  participant F as ClientsPage
  participant C as ClientsController
  participant S as ClientService
  participant R as ClientRepository
  participant D as PostgreSQL

  A->>F: вводит "иванов" в поиск
  F->>F: задержка 400 мс
  F->>C: GET /api/clients?search=иванов&page=1&pageSize=20
  C->>S: GetPagedAsync(query)
  S->>S: обрезка пробелов, pageSize не более 100
  S->>R: GetPagedAsync(нормализованный query)
  R->>R: сборка IQueryable из фильтров
  R->>D: SELECT count(*) с учётом фильтров
  D-->>R: 2
  R->>D: SELECT страница с ORDER BY, LIMIT, OFFSET
  D-->>R: строки
  R-->>S: PagedResult
  S-->>C: PagedResult
  C-->>F: 200 + items, totalCount
  F-->>A: таблица с пагинацией
```

Два обращения к базе вместо одного: общее количество считается **до**
`LIMIT`, иначе нельзя показать «всего 50» при загруженных двадцати строках.

---

## 5. Вход и работа с токеном

```mermaid
sequenceDiagram
  actor A as Сотрудник
  participant F as LoginPage
  participant C as AuthController
  participant S as AuthService
  participant R as UserRepository
  participant H as BCryptPasswordHasher
  participant T as JwtTokenGenerator

  A->>F: логин и пароль
  F->>C: POST /api/auth/login
  C->>S: LoginAsync(command)
  S->>R: FindByUsernameAsync
  R-->>S: UserCredentials с хешем

  alt пользователь не найден, отключён или пароль неверен
    S-->>C: BusinessRuleException invalid_credentials
    C-->>F: 401
  else успех
    S->>H: Verify(пароль, хеш)
    H-->>S: true
    S->>T: Generate(user)
    T-->>S: JWT
    C-->>F: 200 + token
    F->>F: сохранение в localStorage
  end
```

Сообщение об ошибке одно на все случаи — иначе перебором можно было бы
выяснить, какие логины существуют.

При фиксации посещения `userId` берётся **из токена**, а не из тела запроса:
иначе любой мог бы записать посещение от чужого имени.

---

## 6. Конвейер обработки запроса

```mermaid
flowchart TD
  REQ["HTTP-запрос"] --> EX["ExceptionHandlingMiddleware"]
  EX --> SW["Swagger, только Development"]
  SW --> CORS["CORS"]
  CORS --> AUTHN["UseAuthentication<br/>кто пришёл"]
  AUTHN --> AUTHZ["UseAuthorization<br/>можно ли ему"]
  AUTHZ --> CTRL["Контроллер"]
  CTRL --> RESP["HTTP-ответ"]
```

Порядок принципиален. Обработчик ошибок стоит первым, потому что ловит
исключения только из того, что идёт ниже. Аутентификация обязана
предшествовать авторизации: сначала выясняем, кто пришёл, потом решаем,
можно ли ему.

### Как исключения превращаются в коды ответов

```mermaid
flowchart LR
  V["ValidationException<br/>FluentValidation"] --> C400["400 + errors"]
  N["NotFoundException"] --> C404["404"]
  B1["BusinessRuleException<br/>invalid_credentials"] --> C401["401"]
  B2["BusinessRuleException<br/>остальные"] --> C409["409 + code"]
  U["Любое другое"] --> C500["500, детали только в лог"]
```

Текст непредвиденной ошибки наружу не отдаётся: он может раскрыть имена
таблиц, фрагменты SQL или содержимое базы.
