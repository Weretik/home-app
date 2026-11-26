using BuildingBlocks.Domain.Results;

namespace Shopping.Domain.Departments;

public static class DepartmentErrors
{
    public static readonly Error NameRequired =
        Error.Validation("shopping.department_name_required", "The department needs a name.", "name");

    public static readonly Error NameTooLong =
        Error.Validation("shopping.department_name_too_long", $"The department name is longer than {Department.MaxNameLength} characters.", "name");

    public static readonly Error IconTooLong =
        Error.Validation("shopping.department_icon_too_long", $"The icon is longer than {Department.MaxIconLength} characters.", "icon");

    public static readonly Error NameTaken =
        Error.Conflict("shopping.department_name_taken", "There already is a department with this name.");

    public static readonly Error FallbackCannotBeDeleted = Error.Conflict(
        "shopping.fallback_department", "The fallback department («Другое») cannot be deleted; items without a department go there.");

    public static readonly Error OrderIncomplete = Error.Validation(
        "shopping.department_order_incomplete", "Send every department exactly once.", "departmentIds");

    public static readonly Error KeywordTooShort =
        Error.Validation("shopping.keyword_too_short", $"A keyword has at least {DepartmentKeyword.MinKeywordLength} characters.", "keyword");

    public static readonly Error KeywordTooLong =
        Error.Validation("shopping.keyword_too_long", $"A keyword is at most {DepartmentKeyword.MaxKeywordLength} characters.", "keyword");

    public static readonly Error KeywordTaken =
        Error.Conflict("shopping.keyword_taken", "This keyword already exists.");

    public static Error NotFound(Guid id) =>
        Error.NotFound("shopping.department_not_found", $"There is no department {id}.");

    public static Error KeywordNotFound(Guid id) =>
        Error.NotFound("shopping.keyword_not_found", $"There is no keyword {id}.");
}
