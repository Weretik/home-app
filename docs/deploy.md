# Deployment

Everything is on free tiers, in the Azure resource group `home-rg`.

| Part | Resource | Address | Workflow |
| --- | --- | --- | --- |
| API | App Service F1 Linux `vitaliy-home-api`, .NET 10 | https://vitaliy-home-api-ase3bfeab4e9gren.polandcentral-01.azurewebsites.net | [.github/workflows/api.yml](../.github/workflows/api.yml) |
| Web | Static Web App Free `home-web` | https://ambitious-beach-0b1d13b10.6.azurestaticapps.net | [.github/workflows/web.yml](../.github/workflows/web.yml) |

Each workflow runs when its folder changes: on a PR only build and tests, on `main` also the deployment. To run one by
hand: Actions → the workflow → Run workflow.

## GitHub Actions secrets

Settings → Secrets and variables → Actions:

| Name | Kind | Where it comes from |
| --- | --- | --- |
| `AZURE_CLIENT_ID` | secret | the Entra ID app used to sign in from GitHub (commands below) |
| `AZURE_TENANT_ID` | secret | `az account show --query tenantId -o tsv` |
| `AZURE_SUBSCRIPTION_ID` | secret | `az account show --query id -o tsv` |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | secret | portal: Static Web App `home-web` → Overview → Manage deployment token |
| `VITE_API_URL` | variable, optional | the API address; when not set, the address above is used |

The API deploys through OIDC: GitHub gets a short-lived Azure token, with no passwords or publish profile (basic auth on
the App Service is off, and that is fine). Sign-in is allowed only from this repository's `production` environment.

### Once: sign-in from GitHub to Azure

In Azure Cloud Shell (Bash):

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

## App Service variables

Portal: App Service `vitaliy-home-api` → Settings → Environment variables:

| Name | Value | When |
| --- | --- | --- |
| `Cors__Origins__0` | `https://ambitious-beach-0b1d13b10.6.azurestaticapps.net` | now, otherwise the front cannot reach the API |
| `ConnectionStrings__Default` | PostgreSQL connection string | once the database is chosen |

Or with one command:

```bash
az webapp config appsettings set -g home-rg -n vitaliy-home-api \
  --settings Cors__Origins__0=https://ambitious-beach-0b1d13b10.6.azurestaticapps.net
```

The startup command (`dotnet HomeApp.Api.dll`) is set by the workflow itself. Turn on HTTPS Only in General settings.

## Database (Neon)

The API reads one connection string, `ConnectionStrings:Default`, in the Npgsql key/value format. When it is set,
`/health` also checks the database.

1. Create a free project at [neon.tech](https://neon.tech) in the region closest to Poland Central (AWS Frankfurt,
   `eu-central-1`). The default database `neondb` is fine.
2. Dashboard → Connect: pick the pooled connection and the **.NET** format. It looks like:
   `Host=ep-xxx-pooler.eu-central-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=...;SSL Mode=Require`
3. Put it into the App Service, either way works:
   - Settings → Environment variables → **App settings**: name `ConnectionStrings__Default` (two underscores).
   - Settings → Environment variables → **Connection strings**: name `Default`, type **Custom**. Do not pick the
     PostgreSQL type: it becomes a `POSTGRESQLCONNSTR_` variable that ASP.NET Core does not map to
     `ConnectionStrings:Default`.
4. Save (the app restarts) and open `/health`: `checks.database` should be `Healthy`.

Locally keep using the Docker database from `api/compose.yaml`, or point at a Neon branch with
`dotnet user-secrets set "ConnectionStrings:Default" "<string>"` in `api/src/HomeApp.Api`. Never commit the string.

## F1 limits

- No Always On: after idle time the first request wakes the app up in 10–30 seconds. The workflow's smoke test allows
  for that.
- 60 CPU minutes per day. Plenty for two people.
- No deployment slots: a deployment goes straight to production; roll back by re-running the workflow on an older
  commit.
