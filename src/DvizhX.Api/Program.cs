using DvizhX.Application.Features.Auth.Commands.Register;
using DvizhX.Infrastructure;
using DvizhX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add DB Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddInfrastructure();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterCommand).Assembly));

// Swashbuckle генерирует сам документ OpenAPI (swagger.json)
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-migration at startup
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//    // Совет: оберни в try-catch, чтобы не падать при старте, если БД еще недоступна
//    try
//    {
//        db.Database.Migrate();
//    }
//    catch (Exception ex)
//    {
//        // Логируем ошибку, но даем приложению упасть, так как без БД работать нельзя
//        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
//        logger.LogError(ex, "An error occurred while migrating the database.");
//        throw;
//    }
//}

if (app.Environment.IsDevelopment())
{
    // Генерируем JSON-спецификацию по адресу /swagger/v1/swagger.json
    app.UseSwagger(options =>
    {
        // Опционально: можно настроить путь к json, если нужно нестандартно
        // options.RouteTemplate = "openapi/{documentName}.json";
    });

    // 2. Подключаем Scalar UI вместо SwaggerUI
    app.MapScalarApiReference(options =>
    {
        // Указываем Scalar'у, где брать спецификацию, сгенерированную Swashbuckle'ом
        // По умолчанию Swashbuckle кладет её сюда:
        options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");

        options
            .WithTitle("DvizhX API Documentation")
            .WithTheme(ScalarTheme.DeepSpace) // Или Mars, Moon, Solarized
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.MapControllers();
app.Run();