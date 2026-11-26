# 0002. Хостинг на бесплатных тарифах Azure

**Статус:** принято, 2026-11-26

## Решение

- API: App Service F1 Linux `vitaliy-home-api` (Poland Central), .NET 10.
- Фронт: Static Web App Free `home-web`.
- Деплой из GitHub Actions: API через OIDC (`azure/login`, без паролей), фронт через deployment token.
- Фронт вызывает API по полному адресу (`VITE_API_URL`), API разрешает origin фронта через CORS: бесплатный Static Web
  App не умеет связываться с отдельным App Service.
- Секреты: переменные App Service и секреты GitHub Actions, локально `dotnet user-secrets`.

## Последствия

Платить ничего не нужно. Цена: холодный старт API после простоя, лимит CPU в сутки, деплой без слотов. Фронт не
использует ничего специфичного для Azure, кроме `staticwebapp.config.json`, поэтому его можно перенести на любой
статический хостинг, поменяв только workflow.
