using Expenses.Domain.Budget;

namespace Expenses.Infrastructure.Persistence.Reference;

/// <summary>
/// The limits of the old «Бюджет» sheet on the day of the move, with fixed ids so the migration that inserts them is
/// stable. Later changes are made on the «Настройки» page, not here.
/// </summary>
internal static class BudgetSeed
{
    public const decimal MonthlyLimit = 50_000m;
    public const decimal SavingsGoal = 100_000m;

    private static readonly Guid Groceries = new("86c6ddca-a991-5d9b-97b9-468b90f50c4c");
    private static readonly Guid Household = new("12b7f2d2-0f0c-5d11-9691-86b3bc919ffe");
    private static readonly Guid PersonalVitaliy = new("35252225-cd52-5360-b8a6-c1d16d8fe1e9");
    private static readonly Guid PersonalMasha = new("e423ab6d-bafe-5a11-a1b0-fbfc88e4bb08");
    private static readonly Guid Utilities = new("77ad0660-c002-50f6-ad87-b04c1762cf13");
    private static readonly Guid Cafes = new("2e1a44f6-fdf2-54ba-998b-ed490c80098d");
    private static readonly Guid Health = new("361658b1-d605-57e0-b6e6-37f5c8c80e7f");
    private static readonly Guid Clothes = new("a064005b-fd96-5454-b17a-8bf70ff694f2");
    private static readonly Guid Repairs = new("eec40aeb-c2a3-5aea-93e8-d4f4929c79aa");
    private static readonly Guid Gifts = new("02d964e6-9360-5e7f-9685-6133d03031ef");
    private static readonly Guid Care = new("dbd79e28-606e-5efb-bc73-f294ba6d0d5d");
    private static readonly Guid Pets = new("08305dd6-28c2-59a1-9d98-0026828b4eee");
    private static readonly Guid Furniture = new("35a6593b-5d73-5fcb-aea8-a2bb9f8e8eb2");
    private static readonly Guid RentedFlat = new("1ab0947d-0601-555e-8bc2-4b4996cbbbe7");

    public static readonly IReadOnlyList<(Guid Id, string Name, LimitPeriod Period, decimal Amount, List<Guid> CategoryIds, int SortOrder)> Limits =
    [
        (new Guid("3f1d6c1e-6b1a-4b3e-9a51-0b6f6c0e1a01"), "Еда и быт", LimitPeriod.Month, 15_000m, [Groceries, Household], 10),
        (new Guid("3f1d6c1e-6b1a-4b3e-9a51-0b6f6c0e1a02"), "Личные Виталий", LimitPeriod.Month, 7_000m, [PersonalVitaliy], 20),
        (new Guid("3f1d6c1e-6b1a-4b3e-9a51-0b6f6c0e1a03"), "Личные Маша", LimitPeriod.Month, 13_000m, [PersonalMasha], 30),
        (new Guid("3f1d6c1e-6b1a-4b3e-9a51-0b6f6c0e1a04"), "Кафе, доставка, развлечения", LimitPeriod.Month, 3_000m, [Cafes], 40),
        (new Guid("3f1d6c1e-6b1a-4b3e-9a51-0b6f6c0e1a05"), "Уход и косметика", LimitPeriod.Quarter, 3_000m, [Care], 50),
        (new Guid("3f1d6c1e-6b1a-4b3e-9a51-0b6f6c0e1a06"), "Зоотовары", LimitPeriod.Quarter, 2_500m, [Pets], 60),
        (new Guid("3f1d6c1e-6b1a-4b3e-9a51-0b6f6c0e1a07"), "Другое", LimitPeriod.Month, 15_000m,
            [Utilities, Cafes, Health, Clothes, Repairs, Gifts, Care, Pets, Furniture, RentedFlat], 70),
    ];
}
