using DvizhX.Api.Hubs;
using DvizhX.Api.Services;
using DvizhX.Application.Common.Interfaces;
using DvizhX.Application.Common.Interfaces.Realtime;
using DvizhX.Application.Features.Auth.Commands.Register;
using DvizhX.Infrastructure;
using DvizhX.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add DB Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor(); // Обязательно для доступа к HttpContext
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IKanbanNotifier, KanbanNotifier>();

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddInfrastructure();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterCommand).Assembly));

// Swashbuckle генерирует сам документ OpenAPI (swagger.json)
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "DvizhX API", 
        Version = "v1",
        Description = "API для управления событиями и совместными задачами",
        Contact = new OpenApiContact
        {
            Name = "DvizhX Team",
            Email = "support@dvizhx.app"
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Описываем схему авторизации
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    // Применяем требование безопасности глобально
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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

    // Добавляем обработку событий для WebSocket
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // Если запрос пришел на хаб и есть токен в URL — забираем его
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
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
        options
            .WithOpenApiRoutePattern("/swagger/v1/swagger.json")
            .WithTitle("DvizhX API Documentation")
            .WithTheme(ScalarTheme.DeepSpace) // Или Mars, Moon, Solarized
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

        options.AddPreferredSecuritySchemes("Bearer");
    });
}

app.UseCors(policy =>
    policy.WithOrigins(
            "https://shulgan-lab.ru",
            "https://www.shulgan-lab.ru",
            "http://localhost:5000",
            "http://localhost:5173" // <--- ДОБАВЛЕНО: Стандартный порт Vite
          )
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials());

app.UseAuthentication(); // Распознает токен и заполняет User
app.UseAuthorization();  // Проверяет права доступа ([Authorize])

app.MapControllers();

app.MapHub<KanbanHub>("/hubs/kanban");

app.Run();