using BuildingBlocks.Domain.Results;

namespace Expenses.Application.Features.Imports.Commands.ImportSheet;

public static class ImportErrors
{
    public const string InvalidRowsCode = "expenses.import_invalid_rows";

    public static readonly Error AlreadyRunning = Error.Conflict(
        "expenses.import_already_running", "Another import stored some of these rows at the same moment; run it again.");
}
