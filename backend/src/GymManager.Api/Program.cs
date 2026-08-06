using System.Text;
using FluentValidation;
using GymManager.Api.Middleware;
using GymManager.Application;
using GymManager.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

// В файле верхнеуровневых операторов ВСЕ using обязаны идти до первой
// исполняемой строки: компилятор оборачивает код в метод Main невидимого
// класса Program, а using относится к файлу целиком.
//
// Пространство имён — Microsoft.OpenApi, а НЕ Microsoft.OpenApi.Models:
// в Swashbuckle 10.x (Microsoft.OpenApi 2.x) вложенное Models упразднено.

var builder = WebApplication.CreateBuilder(args);

// --- Слои ------------------------------------------------------------------
// Каждый слой регистрирует себя сам: Program.cs не знает про ClientService
// и ClientRepository, только про два метода расширения.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

// Сканирует сборку и находит все классы, унаследованные от AbstractValidator<>.
// Новый валидатор подхватится сам, без правки этого файла.
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// --- Аутентификация --------------------------------------------------------
var jwt = builder.Configuration.GetSection("Jwt");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Проверяем, что токен выпустили мы, для нас, не истёк
            // и подписан нашим ключом.
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!)),

            // По умолчанию допускается расхождение часов в 5 минут,
            // из-за чего истёкший токен ещё какое-то время работает.
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// --- Swagger ---------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GymManager API",
        Version = "v1",
        Description = "Учёт клиентов, абонементов и посещений фитнес-центра."
    });

    // Подтягиваем XML-комментарии из кода в документацию.
    // Требует <GenerateDocumentationFile>true</GenerateDocumentationFile>
    // в GymManager.Api.csproj — иначе файла просто не будет.
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    // Кнопка Authorize в интерфейсе Swagger: без неё защищённые эндпоинты
    // невозможно попробовать прямо из браузера.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите только сам токен, без слова Bearer."
    });

    // В Microsoft.OpenApi 2.x ссылка на схему задаётся типом
    // OpenApiSecuritySchemeReference вместо прежней конструкции
    // с OpenApiReference и ReferenceType.
    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
    });
});

// --- CORS для фронтенда на Vite --------------------------------------------
const string FrontendCors = "frontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCors, policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

// --- Конвейер обработки запроса --------------------------------------------
// ПОРЯДОК ВАЖЕН.

// Обработчик ошибок первым: он ловит исключения только из того,
// что идёт НИЖЕ него в конвейере.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(FrontendCors);

// Сначала выясняем, КТО пришёл, потом — МОЖНО ли ему.
// Поменяешь местами — авторизация будет решать про анонима.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
