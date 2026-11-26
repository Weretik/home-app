# 00 — Readiness

## EN-001 — Module skeleton

- Paths: `src/Modules/Shopping/Shopping.{Api,Application,Contracts,Domain,Infrastructure}/*.csproj`,
  `HomeApp.slnx`, `src/Host/Host.Api/{Program.cs,Host.Api.csproj,Composition/MediatorSetup.cs}`,
  `tests/UnitTests/UnitTests.csproj`.
- Work: project references as Expenses (Api → Application + Infrastructure; Infrastructure → Application;
  Application → Domain + Contracts); `InternalsVisibleTo UnitTests` on Application.
- Test-first exception: no behaviour yet. Substitute check: `dotnet build HomeApp.slnx`;
  `ModuleBoundaryTests.Every_Module_HasAllFiveLayers` and `LayerDependencyTests` pick up `Shopping` by folder.
- Scope check: the brief is the source of truth; the seed comes from the live sheet via the Google Sheets tool,
  never invented.
- Evidence: build green, architecture tests green with the module listed.
