Запуск локального контейнера с базой:
docker compose -f docker-compose.dev.yml up -d

Миграции:
dotnet ef migrations add InitialCreate -s src/DvizhX.Api -p src/DvizhX.Infrastructure
dotnet ef database update -s src/DvizhX.Api -p src/DvizhX.Infrastructure