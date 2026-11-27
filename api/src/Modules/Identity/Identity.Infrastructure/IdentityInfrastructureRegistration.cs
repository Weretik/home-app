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
            new FamilyAllowlist(AllowedEmails(section.GetSection("AllowedEmails")))));
        services.AddSingleton<IGoogleTokenVerifier, GoogleTokenVerifier>();
        return services;
    }

    /// <summary>
    /// The entries by their index (<c>AllowedEmails:0</c>, <c>:1</c>, ...), a missing index read as empty, so an entry
    /// keeps its position and with it the person it stands for (ADR 0038).
    /// </summary>
    private static IEnumerable<string?> AllowedEmails(IConfigurationSection section)
    {
        var last = section.GetChildren()
            .Select(child => int.TryParse(child.Key, out var index) ? index : -1)
            .DefaultIfEmpty(-1)
            .Max();
        return Enumerable.Range(0, last + 1).Select(index => section[index.ToString(System.Globalization.CultureInfo.InvariantCulture)]);
    }
}
