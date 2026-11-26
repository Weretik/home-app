using BuildingBlocks.Api.Health;
using BuildingBlocks.Api.Scheduling;
using Expenses.Api;
using Shopping.Api;
using Gifts.Api;
using Host.Api.Composition;
using Notifications.Api;
using Todos.Api;

var builder = WebApplication.CreateBuilder(args);

// In every environment, not only Development: a singleton that captures a scoped service (a DbContext) or a missing
// registration fails at startup instead of on the first request.
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// Each module adds itself here: .Add<Module>Module(builder.Configuration).
builder.Services.AddPlatform(builder.Configuration);
builder.Services.AddExpensesModule(builder.Configuration);
builder.Services.AddShoppingModule(builder.Configuration);
builder.Services.AddGiftsModule(builder.Configuration);
builder.Services.AddTodosModule(builder.Configuration);
builder.Services.AddNotificationsModule(builder.Configuration);

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
app.MapShoppingModule();
app.MapGiftsModule();
app.MapTodosModule();
app.MapNotificationsModule();

await app.RunAsync();

/// <summary>Lets the integration tests start the real host.</summary>
public partial class Program;
