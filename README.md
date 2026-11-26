# Дом

Семейный сайт вместо таблиц Google и ботов на Apps Script: расходы, список покупок, дела (Маша, Виталий, общее) и
список подарков. Сейчас в репозитории только каркас: API с `/health` и страница, которая его вызывает.

**Стек:** .NET 10 · ASP.NET Core · PostgreSQL (провайдер ещё не выбран) · React 19 · Vite · TypeScript

```
api/    ASP.NET Core API, один проект, фичи по папкам (api/src/HomeApp.Api/Features/<Область>/)
web/    React + Vite, обычная статическая сборка в web/dist
docs/   Архитектура, деплой, решения (ADR)
```

`api/` и `web/` не зависят друг от друга: фронт знает только адрес API (`VITE_API_URL`), API разрешает origin фронта
через CORS. У каждой половины свой workflow, он запускается только когда меняется её папка.

| | Где |
| --- | --- |
| Как устроено | [docs/architecture.md](docs/architecture.md) |
| Деплой и секреты | [docs/deploy.md](docs/deploy.md) |
| Решения | [docs/decisions/README.md](docs/decisions/README.md) |
| Правила для ИИ-агентов | [AGENTS.md](AGENTS.md) |

## Запуск локально

Нужны .NET 10 SDK, Node.js 22 и Docker (для базы).

```bash
# База (необязательно: без неё /health покажет database: Unhealthy)
cd api
docker compose up -d

# API на http://localhost:5000
dotnet run --project src/HomeApp.Api

# Фронт на http://localhost:5173 (во втором терминале)
cd web
cp .env.example .env.local
npm ci
npm run dev
```

Секреты локально хранятся в `dotnet user-secrets` (`cd api/src/HomeApp.Api && dotnet user-secrets set "Ключ" "значение"`),
в репозиторий они не попадают.

## Проверки

```bash
cd api && dotnet test HomeApp.slnx
cd web && npm run typecheck && npm test && npm run format:check
```
