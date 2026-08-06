using FluentValidation;
using GymManager.Api.Middleware;
using GymManager.Application;
using GymManager.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Слои регистрируют себя сами: Program.cs не знает про ClientService
// и ClientRepository, только про два метода расширения.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

// Сканирует сборку и находит все классы, унаследованные от AbstractValidator<>.
// Новый валидатор подхватится сам, без правки этого файла.
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// ПОРЯДОК ВАЖЕН: middleware ловит исключения только из того, что идёт НИЖЕ
// него в конвейере. После MapControllers работать не будет.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
