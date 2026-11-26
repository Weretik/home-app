using BuildingBlocks.Api.Health;
using BuildingBlocks.Api.Scheduling;
using Host.Api.Composition;

var builder = WebApplication.CreateBuilder(args);

// Each module adds itself here: .Add<Module>Module(builder.Configuration).
builder.Services.AddPlatform(builder.Configuration);

var app = builder.Build();

app.UseReverseProxySupport();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors(CorsSetup.PolicyName);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapApiDocumentation();
app.MapHealth();
app.MapOutboxTick();
// Each module maps its endpoints here: app.Map<Module>Module().

await app.RunAsync();

/// <summary>Lets the integration tests start the real host.</summary>
public partial class Program;
