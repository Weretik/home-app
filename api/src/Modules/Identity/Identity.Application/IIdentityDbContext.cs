using Identity.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application;

/// <summary>The module's unit of work: handlers query the sessions and call <see cref="SaveChangesAsync"/> once.</summary>
public interface IIdentityDbContext
{
    DbSet<Session> Sessions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
