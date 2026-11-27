using BuildingBlocks.Domain.Results;

namespace Shopping.Domain.Departments;

public static class DepartmentErrors
{
    public static readonly Error NameRequired =
        Error.Validation("shopping.department_name_required", "Відділу потрібна назва.", "name");

    public static readonly Error NameTooLong =
        Error.Validation("shopping.department_name_too_long", $"Назва відділу задовга: не більше {Department.MaxNameLength} символів.", "name");

    public static readonly Error IconTooLong =
        Error.Validation("shopping.department_icon_too_long", $"Іконка задовга: не більше {Department.MaxIconLength} символів.", "icon");

    public static readonly Error NameTaken =
        Error.Conflict("shopping.department_name_taken", "Відділ із такою назвою вже є.");

    public static readonly Error FallbackCannotBeDeleted = Error.Conflict(
        "shopping.fallback_department", "Відділ за замовчуванням («Другое») видалити не можна: туди потрапляють товари без відділу.");

    public static readonly Error OrderIncomplete = Error.Validation(
        "shopping.department_order_incomplete", "Надішліть кожен відділ рівно один раз.", "departmentIds");

    public static readonly Error KeywordTooShort =
        Error.Validation("shopping.keyword_too_short", $"Ключове слово закоротке: щонайменше {DepartmentKeyword.MinKeywordLength} символи.", "keyword");

    public static readonly Error KeywordTooLong =
        Error.Validation("shopping.keyword_too_long", $"Ключове слово задовге: не більше {DepartmentKeyword.MaxKeywordLength} символів.", "keyword");

    public static readonly Error KeywordTaken =
        Error.Conflict("shopping.keyword_taken", "Таке ключове слово вже є.");

    public static Error NotFound(Guid id) =>
        Error.NotFound("shopping.department_not_found", $"Відділу {id} немає.");

    public static Error KeywordNotFound(Guid id) =>
        Error.NotFound("shopping.keyword_not_found", $"Ключового слова {id} немає.");
}
