# 🚀 DvizhX (Backend)

> **DvizhX** — это платформа для организации тусовок, поездок и встреч. Создавай события, приглашай друзей, планируй задачи на Канбан-доске и дели траты.
> *Кодовое имя: Бухикс24*

![Status](https://img.shields.io/badge/Status-In%20Development-yellow)
![Tech](https://img.shields.io/badge/Stack-.NET%2010%20%7C%20PostgreSQL%20%7C%20SignalR-blue)

## 📋 Оглавление
- [Функционал](#-функционал)
- [Быстрый старт](#-быстрый-старт)
- [Архитектура](#-архитектура)
- [Документация API](#-документация-api)
- [Планы (Roadmap)](#-roadmap--todo)

---

## 🛠 Технологический стек
*   **Framework:** .NET 10 (ASP.NET Core Web API)
*   **Database:** PostgreSQL (EF Core Code First)
*   **Auth:** JWT (Access + Refresh Tokens), Google Auth, Telegram Widget
*   **Real-time:** SignalR (WebSockets)
*   **Docs:** Scalar (OpenAPI/Swagger)
*   **Architecture:** Clean Architecture + CQRS (MediatR)

---

## 🚀 Быстрый старт

### 1. Предварительные требования
*   Docker Desktop
*   .NET 10 SDK

### 2. Запуск инфраструктуры (БД + Adminer)
Поднимает PostgreSQL и pgAdmin в контейнерах.

docker compose -f docker-compose.dev.yml up -d

*   **Postgres:** localhost:5432 (user: developer, password: developer, db: dev_main)
*   **pgAdmin:** http://localhost:5050 (email: admin@admin.com, password: admin)

### 3. Применение миграций
Создает таблицы в базе данных. Выполнять из корня репозитория:

# Если миграция еще не создана:
dotnet ef migrations add InitialCreate -s src/DvizhX.Api -p src/DvizhX.Infrastructure

# Если не видит dotnet ef:
dotnet tool install --global dotnet-ef
$env:PATH = $env:PATH + ";C:\Users\{username}\.dotnet\tools"

# Применить миграции:
dotnet ef database update -s src/DvizhX.Api -p src/DvizhX.Infrastructure

### 4. Запуск API
dotnet run --project src/DvizhX.Api

**API запустится на http://localhost:5xxx (см. вывод консоли).**

---

## 📚 Документация API

После запуска перейдите по адресу:
👉 **[http://localhost:5000/scalar/v1](http://localhost:5000/scalar/v1)**

Там доступен интерактивный UI для тестирования всех эндпоинтов.
*   Для авторизации используйте кнопку **Auth** и введите Access Token.
*   Токен можно получить через \`POST /api/auth/register\` или \`login\`.

---

## 📡 SignalR (Real-time)

Хаб доступен по адресу: **/hubs/kanban**
Для подключения требуется Access Token в Query Params: **?access_token=YOUR_JWT**

**Методы клиента:**
*   JoinBoard(Guid eventId) — Подписаться на обновления доски.
*   LeaveBoard(Guid eventId) — Отписаться.

**События сервера:**
*   CardCreated — Прилетает CardDto, когда кто-то создал задачу.
*   CardMoved — Прилетает { cardId, newColumnId, newIndex }, когда задачу перетащили.

---

## 🗺 Roadmap & TODO

### ✅ Готово (MVP Phase 1)
- [x] **Infrastrucutre:** Docker Compose, EF Core, Clean Architecture Setup
- [x] **Auth:** Регистрация, Логин, Refresh Tokens (Rotation)
- [x] **Social Auth:** Вход через Google и Telegram
- [x] **Events:** Создание, Просмотр списка, Детали события (с проверкой прав)
- [x] **Invites:** Вступление в событие по инвайт-коду (/join/{code})
- [x] **Kanban (Base):** Авто-создание доски, Просмотр колонок/задач
- [x] **Kanban (Write):** Создание карточки + SignalR уведомление

### 🚧 В работе (MVP Phase 2)
- [ ] **Kanban:** Перемещение карточек (MoveCard) с пересчетом индексов.
- [ ] **Kanban:** Редактирование и удаление задач.
- [ ] **Users:** Загрузка аватарок (интеграция с MinIO/S3).
- [ ] **Events:** Редактирование события, генерация красивых ссылок.

### 💡 Идеи на будущее (Backlog)
- [ ] **Finance Module:** "Кто кому должен" (Splitwise аналог).
- [ ] **Checklists:** Подзадачи внутри карточки с галочками.
- [ ] **Chat:** Общий чат внутри события (тоже на SignalR).
- [ ] **Notifications:** Telegram-бот, который шлет пуши "Вася добавил задачу".
- [ ] **Export:** Выгрузка итогов поездки (фото + финансы) в PDF/Zip.
- [ ] **Gamification:** Ачивки ("Шашлычный мастер", "Главный спонсор").

---

## 🔧 Полезные команды

**Очистка проекта (если что-то сломалось):**
dotnet clean
dotnet restore
dotnet build

**Добавление новой миграции:**
dotnet ef migrations add <Name> -s src/DvizhX.Api -p src/DvizhX.Infrastructure

---

## 👥 Команда
- **Backend (3):** Архитектура, API, БД, SignalR
- **Frontend (1):** React PWA, UI/UX

---

## 📄 Лицензия
MIT (или укажите вашу)
EOF