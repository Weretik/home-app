using HomeApp.Api.Platform;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddProblemDetails()
    .AddOpenApi()
    .AddCorsForWeb(builder.Configuration)
    .AddDatabase(builder.Configuration)
    .AddHealth(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors(CorsSetup.PolicyName);

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapHealth();

await app.RunAsync();

/// <summary>Lets the tests start the real host.</summary>
public partial class Program;
