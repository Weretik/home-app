using BuildingBlocks.Api.Health;
using BuildingBlocks.Api.Scheduling;
using Expenses.Api;
using Gifts.Api;
using Host.Api.Composition;
using Todos.Api;

var builder = WebApplication.CreateBuilder(args);

// Each module adds itself here: .Add<Module>Module(builder.Configuration).
builder.Services.AddPlatform(builder.Configuration);
builder.Services.AddExpensesModule(builder.Configuration);
builder.Services.AddGiftsModule(builder.Configuration);
builder.Services.AddTodosModule(builder.Configuration);

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
app.MapExpensesModule();
app.MapGiftsModule();
app.MapTodosModule();

await app.RunAsync();

/// <summary>Lets the integration tests start the real host.</summary>
public partial class Program;
