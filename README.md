<h1 align="center">Репозиторий проекта DvizhX (Бухикс24)</a>
<br/>
<h3 align="center">Запуск локальной БД:</h3>
<h4 align="center">docker compose -f docker-compose.dev.yml up -d</h4>
<br/>
<h3 align="center">Создание и применение миграций</h3>
<h4 align="center">dotnet ef migrations add InitialCreate -s src/DvizhX.Api -p src/DvizhX.Infrastructure</h4>
<h4 align="center">dotnet ef database update -s src/DvizhX.Api -p src/DvizhX.Infrastructure</h4>

