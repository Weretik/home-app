namespace Expenses.Api.Endpoints.Import;

/// <summary>
/// <c>Expenses:Import:Key</c>: the shared secret of the one-time sheet import. Empty (the default) switches the
/// endpoint off; set it only for the import and clear it afterwards.
/// </summary>
public sealed class SheetImportOptions
{
    public const string SectionName = "Expenses:Import";

    public string Key { get; set; } = string.Empty;
}
