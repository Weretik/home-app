using BuildingBlocks.Domain.Results;
using Mediator;
using Shopping.Application.Features.Dictionary;

namespace Shopping.Application.Features.Departments.Queries.MatchDepartment;

public sealed class MatchDepartmentHandler(IShoppingDbContext db) : IQueryHandler<MatchDepartmentQuery, Result<DepartmentMatch>>
{
    public async ValueTask<Result<DepartmentMatch>> Handle(MatchDepartmentQuery query, CancellationToken cancellationToken)
    {
        var dictionary = await DepartmentDictionary.LoadAsync(db, cancellationToken);
        var department = dictionary.Departments[dictionary.Matcher.Match(query.Name!)];
        return new DepartmentMatch(department.Id, department.Name, department.Icon);
    }
}
