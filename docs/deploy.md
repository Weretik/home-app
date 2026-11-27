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
| `DATABASE_CONNECTION_STRING` | secret of the `production` environment | the same string as the App Service's `ConnectionStrings__Database`; the deploy job applies migrations with it |
| `SCHEDULER_KEY` | secret | a long random value, the same as the App Service's `OutboxTick__Key`; the `scheduler.yml` cron sends it every 15 minutes. Until it is set the cron only prints a notice ([ADR 0034](../api/docs/decisions/0034-scheduler-github-actions-cron.md)). The deploy also reads every section with it ([ADR 0037](../api/docs/decisions/0037-google-sign-in-with-server-sessions.md)), so the deploy fails without it |
| `VITE_GOOGLE_CLIENT_ID` | variable | the Google OAuth client id ([below](#sign-in-with-google)); public, it ends up in the JS |

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
| `ConnectionStrings__Database` | PostgreSQL connection string | once the database is chosen |
| `Expenses__Import__Key` | long random value | normally never: the sheet is imported on a local API and the rows reach production as a SQL script; set it only for a direct import, then delete it ([module page](../api/docs/modules/expenses.md#the-one-time-import-from-the-sheet)) |
| `OutboxTick__Key` | long random value, equal to the GitHub secret `SCHEDULER_KEY` | for the outbox and the Telegram bot; without it both ticks answer `404` |
| `Telegram__BotToken` | the token from @BotFather | for the Telegram bot; empty means the bot is off and the webhook answers `404` |
| `WebPush__PublicKey`, `WebPush__PrivateKey` | the VAPID key pair ([below](#push-notifications)) | for push notifications; without them push is off and the «Сповіщення» page says so |
| `Identity__Google__ClientId` | the Google OAuth client id ([below](#sign-in-with-google)) | always; without it nobody can sign in and the site stays closed |
| `Identity__AllowedEmails__0`, `Identity__AllowedEmails__1` | Vitaliy's (`__0`) and Masha's (`__1`) Google e-mails | always; only these may sign in, and the index says who is who (pages start on that person, API ADR 0038). Removing one locks that person out at once |

Or with one command:

```bash
az webapp config appsettings set -g home-rg -n vitaliy-home-api \
  --settings Cors__Origins__0=https://ambitious-beach-0b1d13b10.6.azurestaticapps.net
```

The startup command (`dotnet Host.Api.dll`) is set by the workflow itself. Turn on HTTPS Only in General settings.

## Database

The API reads one connection string, `ConnectionStrings:Database`, in the Npgsql key/value format. When it is set,
`/health` also checks the database. Each backend module keeps its tables in its own schema (`shopping`, `todos`, ...;
see [ADR 0003](decisions/0003-postgres-at-hosting-provider.md) and the backend's
[schema-per-module ADR](../api/docs/decisions/0005-schema-per-module.md)).

### Main option: a separate database at the hosting provider (adm.tools)

The hosting plan allows several PostgreSQL databases, so this project gets its own database next to the other ones.

1. **Database.** adm.tools → Databases → Create database (ours is `weretick`). The panel creates an owner user of the
   same name with all rights on it. Set its password with "Change password".
2. **Access from other users (optional).** The panel gives `PUBLIC` the TEMPORARY & CONNECT rights on every database,
   so other database users on the account can connect to it. Remove the `PUBLIC` row on the new database if you want
   only its owner to get in.
3. **Remote access.** Already open: another project reaches this server from Google Cloud Run with changing IPs, so no
   IP allow-list is needed. If the hosting ever restricts it, allow every address from
   `az webapp show -g home-rg -n vitaliy-home-api --query possibleOutboundIpAddresses -o tsv`.
4. **SSL.** Use the same host, port and SSL settings as the other project. With SSL use `SSL Mode=Require`; without
   it the password travels in clear text over the internet, so prefer SSL if at all possible.
5. **Connection string**, then put it into the App Service as described below:

   `Host=brevistore.psql.tools;Port=10403;Database=weretick;Username=weretick;Password=<password>;SSL Mode=Require`

   The host and port are the account's PostgreSQL instance (the same one the other project uses). If the server turns
   out not to support SSL, the connection fails with `SSL Mode=Require`; then use `SSL Mode=Prefer`.

Each module's first migration creates its schema itself, since the owner has all rights; nothing to run by hand.

**Migrations.** Production never migrates on start (`Database:InitializeOnStartup` is off). The API workflow builds
one EF migrations bundle per module (today `identity`, `expenses`, `shopping`, `gifts`, `todos`, `notes` and `notifications`, listed in
`.github/workflows/api.yml`) and, in the
deploy job, runs them against the `DATABASE_CONNECTION_STRING` secret
before the new version is deployed. Without that secret the deploy stops with an error, and the running version stays.
Add it once: Settings → Environments → `production` → Environment secrets → `DATABASE_CONNECTION_STRING` (the GitHub
runner reaches the database because the server accepts remote connections). See
[ADR 0016](../api/docs/decisions/0016-migrations-as-bundles-in-deploy.md).

**Gifts import.** After the first deploy that creates the schema `gifts`, the gift wishlist from the old sheet is
loaded once by running its SQL script by hand (DBeaver) against the production database; every insert is
`ON CONFLICT DO NOTHING`, so a second run changes nothing
([ADR 0026](../api/docs/decisions/0026-gifts-import-as-sql-script.md)).

### Alternative: Neon

1. Create a free project at [neon.tech](https://neon.tech) in AWS Frankfurt (`eu-central-1`), the closest region to
   Poland Central.
2. Dashboard → Connect: pick the pooled connection and the **.NET** format.

### Putting the connection string into the App Service

- Settings → Environment variables → **App settings**: name `ConnectionStrings__Database` (two underscores), or
- Settings → Environment variables → **Connection strings**: name `Database`, type **Custom**. Do not pick the
  PostgreSQL type: it becomes a `POSTGRESQLCONNSTR_` variable that ASP.NET Core does not map to
  `ConnectionStrings:Database`.

Save (the app restarts) and open `/health`: `checks.database` should be `Healthy`.

Locally keep using the Docker database from `api/compose.yaml`, or point at a real one with
`dotnet user-secrets set "ConnectionStrings:Database" "<string>"` in `api/src/Host/Host.Api`. Never commit the string.

## Sign in with Google

Only the e-mails in `Identity__AllowedEmails__*` may use the site
([ADR 0037](../api/docs/decisions/0037-google-sign-in-with-server-sessions.md)). Free, no client secret.

1. [Google Cloud Console](https://console.cloud.google.com) → any project (or a new one) → APIs & Services → OAuth
   consent screen: External, app name «Дом», add both e-mails as test users (or publish the app; the basic
   `openid email profile` scopes need no verification).
2. APIs & Services → Credentials → Create credentials → OAuth client ID → **Web application**. Authorized JavaScript
   origins: `https://ambitious-beach-0b1d13b10.6.azurestaticapps.net` and `http://localhost:5173`. No redirect URIs:
   the web app uses the Google button, not a redirect.
3. App Service variables: `Identity__Google__ClientId` = the client id, `Identity__AllowedEmails__0` and `__1` = the
   two e-mails (`__0` Vitaliy, `__1` Masha). GitHub repository variable `VITE_GOOGLE_CLIENT_ID` = the same client id
   (Settings → Secrets and variables → Actions → **Variables**; a secret of the same name works too). It is baked into
   the web build, so a change needs a new web deploy (re-run the latest «Web» workflow).
4. Set them before merging the sign-in change. The API deploy fails its sign-in smoke step while the API client id is
   missing; the web deploy refuses to build without `VITE_GOOGLE_CLIENT_ID` and, after uploading, checks that the
   site's JS carries it (otherwise the sign-in screen says «Вхід ще не налаштовано»).

Locally: `dotnet user-secrets set "Identity:Google:ClientId" "<id>"` (and `Identity:AllowedEmails:0`) in
`src/Host/Host.Api`, and `VITE_GOOGLE_CLIENT_ID` in `web/.env.local`.

## Telegram bot

Once ([ADR 0033](../api/docs/decisions/0033-notifications-module-and-telegram-webhook.md),
[module page](../api/docs/modules/notifications.md)):

1. In Telegram, @BotFather → `/newbot` → copy the token. Never commit it.
2. Generate a key: `openssl rand -hex 32`. Put it into the App Service as `OutboxTick__Key` and into GitHub as the
   `SCHEDULER_KEY` secret.
3. Put the token into the App Service as `Telegram__BotToken` and restart it.
4. The next scheduler tick registers the webhook itself (`setWebhook` with the secret derived from the token). To do it
   at once, run the `Scheduler` workflow by hand (Actions → Scheduler → Run workflow).
5. On the site, «Уведомления»: open the bot link, press Start in each private chat and choose «Я Виталий» / «Я Маша»;
   add the bot to the family group (it becomes the group chat). «Проверить» sends a test message.

A new token changes the webhook secret; the next tick sets the webhook again.

## Push notifications

Once ([API ADR 0042](../api/docs/decisions/0042-web-push-next-to-telegram.md)): generate a VAPID key pair and put it
into the App Service as `WebPush__PublicKey` and `WebPush__PrivateKey` (the private key is a secret; never commit it).
Any tool that makes a P-256 key pair in base64url works, for example `npx web-push generate-vapid-keys`. A new pair
makes every saved subscription useless: each device has to turn push on again. Then on each phone or computer:
«Сповіщення» → «Сповіщення на цьому пристрої» (on an iPhone first add «Дім» to the home screen and open it from there).

## F1 limits

- No Always On: after idle time the first request wakes the app up in 10–30 seconds. The workflow's smoke test allows
  for that.
- After `/health` shows the new commit, the smoke test also reads the first endpoint of every section (system status,
  expenses summary and budget, shopping, gifts and todos summaries, the notes and their categories, Telegram and push status) and fails the deploy on anything
  but 200 ([ADR 0005](decisions/0005-post-deploy-smoke-reads-every-section.md)). A new section adds its endpoint there.
- 60 CPU minutes per day. Plenty for two people, including the scheduler's 96 short calls a day (it also keeps the
  app warm).
- No deployment slots: a deployment goes straight to production; roll back by re-running the workflow on an older
  commit.
