# Деплой

Всё бесплатное, группа ресурсов `home-rg` в Azure.

| Часть | Ресурс | Адрес | Workflow |
| --- | --- | --- | --- |
| API | App Service F1 Linux `vitaliy-home-api`, .NET 10 | https://vitaliy-home-api-ase3bfeab4e9gren.polandcentral-01.azurewebsites.net | [.github/workflows/api.yml](../.github/workflows/api.yml) |
| Web | Static Web App Free `home-web` | https://ambitious-beach-0b1d13b10.6.azurestaticapps.net | [.github/workflows/web.yml](../.github/workflows/web.yml) |

Каждый workflow запускается при изменении своей папки: на PR только сборка и тесты, на `main` ещё и деплой. Вручную:
Actions → workflow → Run workflow.

## Секреты GitHub Actions

Settings → Secrets and variables → Actions:

| Имя | Тип | Откуда |
| --- | --- | --- |
| `AZURE_CLIENT_ID` | secret | приложение Entra ID для входа из GitHub (команды ниже) |
| `AZURE_TENANT_ID` | secret | `az account show --query tenantId -o tsv` |
| `AZURE_SUBSCRIPTION_ID` | secret | `az account show --query id -o tsv` |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | secret | портал: Static Web App `home-web` → Overview → Manage deployment token |
| `VITE_API_URL` | variable, необязательно | адрес API; если не задан, берётся адрес выше |

API деплоится через OIDC: GitHub получает короткоживущий токен Azure, паролей и publish profile нет (basic auth на
App Service выключен, и это нормально). Вход разрешён только из окружения `production` этого репозитория.

### Один раз: вход из GitHub в Azure

В Azure Cloud Shell (Bash):

```bash
APP_ID=$(az ad app create --display-name home-app-github --query appId -o tsv)
az ad sp create --id "$APP_ID"
SUB=$(az account show --query id -o tsv)
az role assignment create --assignee "$APP_ID" --role "Website Contributor" \
  --scope "/subscriptions/$SUB/resourceGroups/home-rg/providers/Microsoft.Web/sites/vitaliy-home-api"
az ad app federated-credential create --id "$APP_ID" --parameters '{
  "name": "github-home-app-production",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:Weretik/home-app:environment:production",
  "audiences": ["api://AzureADTokenExchange"]
}'
echo "AZURE_CLIENT_ID=$APP_ID"
echo "AZURE_TENANT_ID=$(az account show --query tenantId -o tsv)"
echo "AZURE_SUBSCRIPTION_ID=$SUB"
```

## Переменные App Service

Портал: App Service `vitaliy-home-api` → Settings → Environment variables:

| Имя | Значение | Когда |
| --- | --- | --- |
| `Cors__Origins__0` | `https://ambitious-beach-0b1d13b10.6.azurestaticapps.net` | сейчас, иначе фронт не достучится до API |
| `ConnectionStrings__Default` | строка подключения PostgreSQL | когда выберем базу |

Или одной командой:

```bash
az webapp config appsettings set -g home-rg -n vitaliy-home-api \
  --settings Cors__Origins__0=https://ambitious-beach-0b1d13b10.6.azurestaticapps.net
```

Команда запуска (`dotnet HomeApp.Api.dll`) выставляется самим workflow. В General settings стоит включить HTTPS Only.

## Ограничения F1

- Нет Always On: после простоя первый запрос будит приложение 10–30 секунд. Smoke-тест в workflow это учитывает.
- 60 минут CPU в сутки. Для двух человек хватает с запасом.
- Нет слотов деплоя: деплой сразу в прод, откат через повторный запуск workflow на старом коммите.
