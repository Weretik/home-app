using BuildingBlocks.Infrastructure.Persistence;
using Identity.Application;
using Identity.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure;

/// <summary>The module's tables, all in the <c>identity</c> schema, with their own migration history.</summary>
public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : ModuleDbContext(options, Schema), IIdentityDbContext
{
    public const string Schema = "identity";

    public DbSet<Session> Sessions => Set<Session>();
}
