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
the App Service is off, and that is fine). Sign-in is allowed only from this repository's `production` environment. GitHub puts the owner and repository ids into
the token subject (`repo:Weretik@37252385/home-app@1473327384:...`), so the federated credential must use that exact
form; a plain `repo:Weretik/home-app:...` subject fails with `AADSTS700213`.

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
  "subject": "repo:Weretik@37252385/home-app@1473327384:environment:production",
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

## Database

The API reads one connection string, `ConnectionStrings:Default`, in the Npgsql key/value format. When it is set,
`/health` also checks the database. All tables live in the `home` schema (see
[ADR 0003](decisions/0003-postgres-schema-on-shared-server.md)), so the database can be shared with another project.

### Main option: existing PostgreSQL at the hosting provider (adm.tools)

1. **Schema and user.** Ideally a separate user that owns only the `home` schema. Connected as the database owner
   (panel's phpPgAdmin or `psql`):

   ```sql
   -- If the panel does not let you create roles, create an extra database user in the panel instead,
   -- or as a last resort keep the owner user and run only: CREATE SCHEMA home;
   CREATE ROLE home_app LOGIN PASSWORD '<strong password>';
   GRANT CONNECT ON DATABASE <database> TO home_app;
   CREATE SCHEMA home AUTHORIZATION home_app;
   ALTER ROLE home_app SET search_path = home;
   ```

   `home_app` owns `home`, so migrations can create tables there, and it gets no rights on the other project's tables.
   Shared hosting often gives the database user no `CREATEROLE` right; then the first comment applies.

2. **Remote access.** Already open: the other project reaches this server from Google Cloud Run with changing IPs, so
   no IP allow-list is needed. If the hosting ever restricts it, allow every address from
   `az webapp show -g home-rg -n vitaliy-home-api --query possibleOutboundIpAddresses -o tsv`.

3. **SSL.** Use the same host, port and SSL settings as the other project. With SSL use `SSL Mode=Require`; without
   it the password travels in clear text over the internet, so prefer SSL if at all possible.

4. **Connection string**, then put it into the App Service the same way as below:

   `Host=<host>;Port=5432;Database=<database>;Username=home_app;Password=<password>;SSL Mode=Require;Search Path=home`

### Alternative: Neon

1. Create a free project at [neon.tech](https://neon.tech) in AWS Frankfurt (`eu-central-1`), the closest region to
   Poland Central.
2. Dashboard → Connect: pick the pooled connection and the **.NET** format, and append `;Search Path=home`.

### Putting the connection string into the App Service

- Settings → Environment variables → **App settings**: name `ConnectionStrings__Default` (two underscores), or
- Settings → Environment variables → **Connection strings**: name `Default`, type **Custom**. Do not pick the
  PostgreSQL type: it becomes a `POSTGRESQLCONNSTR_` variable that ASP.NET Core does not map to
  `ConnectionStrings:Default`.

Save (the app restarts) and open `/health`: `checks.database` should be `Healthy`.

Locally keep using the Docker database from `api/compose.yaml`, or point at a real one with
`dotnet user-secrets set "ConnectionStrings:Default" "<string>"` in `api/src/HomeApp.Api`. Never commit the string.

## F1 limits

- No Always On: after idle time the first request wakes the app up in 10–30 seconds. The workflow's smoke test allows
  for that.
- 60 CPU minutes per day. Plenty for two people.
- No deployment slots: a deployment goes straight to production; roll back by re-running the workflow on an older
  commit.
