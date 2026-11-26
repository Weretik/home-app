# 0002. Hosting on Azure free tiers

**Status:** accepted, 2026-11-26

## Decision

- API: App Service F1 Linux `vitaliy-home-api` (Poland Central), .NET 10.
- Front: Static Web App Free `home-web`.
- Deployment from GitHub Actions: the API through OIDC (`azure/login`, no passwords), the front through a deployment
  token.
- The front calls the API by its full address (`VITE_API_URL`) and the API allows the front's origin through CORS: a
  free Static Web App cannot link to a separate App Service.
- Secrets: App Service variables and GitHub Actions secrets, locally `dotnet user-secrets`.

## Consequences

Nothing to pay. The cost: a cold start of the API after idle time, a daily CPU limit, and deployments without slots.
The front uses nothing Azure-specific except `staticwebapp.config.json`, so it can move to any static host by changing
only the workflow.
