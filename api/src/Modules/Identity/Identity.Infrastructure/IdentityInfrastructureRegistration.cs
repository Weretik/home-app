using BuildingBlocks.Infrastructure.Persistence;
using Identity.Application.Features.Members;
using Identity.Application.Google;
using Identity.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure;

public static class IdentityInfrastructureRegistration
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<IdentityDbContext>(configuration, IdentityDbContext.Schema);
        services.AddScoped<IIdentityDbContext>(provider => provider.GetRequiredService<IdentityDbContext>());
        services.AddScoped<IDatabaseInitializer, IdentityDatabaseInitializer>();

        var section = configuration.GetSection(IdentitySettings.SectionName);
        services.AddSingleton(new IdentitySettings(
            section["Google:ClientId"]?.Trim() ?? string.Empty,
            new FamilyAllowlist(section.GetSection("AllowedEmails").GetChildren().Select(email => email.Value ?? string.Empty))));
        services.AddSingleton<IGoogleTokenVerifier, GoogleTokenVerifier>();
        return services;
    }
}
