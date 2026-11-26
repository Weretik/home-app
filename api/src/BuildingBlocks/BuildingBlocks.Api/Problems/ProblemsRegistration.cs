using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Api.Problems;

public static class ProblemsRegistration
{
    /// <summary>RFC 9457 problem details for every error answer, and one handler for unexpected exceptions.</summary>
    public static IServiceCollection AddApiProblems(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }
}
