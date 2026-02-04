using DvizhX.Application.Features.Auth.Commands.Register;
using DvizhX.Infrastructure;
using DvizhX.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

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

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var secret = builder.Configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true, // Проверяем срок действия
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
        ClockSkew = TimeSpan.Zero // Убираем 5-минутную погрешность по умолчанию
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

//Auto - migration at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Совет: оберни в try-catch, чтобы не падать при старте, если БД еще недоступна
    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Логируем ошибку, но даем приложению упасть, так как без БД работать нельзя
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
}

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

app.UseCors(policy =>
    policy.WithOrigins("https://shulgan-lab.ru", "https://www.shulgan-lab.ru", "http://localhost:5000")
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials()); // Важно, если будем передавать куки или заголовки авторизации

app.UseAuthentication(); // Распознает токен и заполняет User
app.UseAuthorization();  // Проверяет права доступа ([Authorize])

app.MapControllers();
app.Run();