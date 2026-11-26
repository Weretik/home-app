using Expenses.Domain.Entries;

namespace Expenses.Api.Endpoints.Planned.Recording;

/// <summary>What a plan needs to become a real expense: the day it was paid, how, and an optional comment.</summary>
internal sealed record RecordPlannedExpenseRequest(DateOnly Date, PaymentMethod PaymentMethod, string? Comment);
