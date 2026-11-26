using Shopping.Application.Features.Departments.Commands.ReorderDepartments;
using Shopping.Application.Features.Items.Queries.ListHistory;
using Shopping.Application.Features.Settings.Commands.UpdateSettings;

namespace UnitTests.Shopping.Input;

public sealed class DictionaryInputValidatorTests
{
    [Fact]
    public void A_Reorder_SendsEachDepartmentOnce()
    {
        var validator = new ReorderDepartmentsValidator();
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();

        Assert.True(validator.Validate(new ReorderDepartmentsCommand([a, b])).IsValid);
        Assert.False(validator.Validate(new ReorderDepartmentsCommand([a, a])).IsValid);
        Assert.False(validator.Validate(new ReorderDepartmentsCommand([])).IsValid);
        Assert.False(validator.Validate(new ReorderDepartmentsCommand([a, Guid.Empty])).IsValid);
    }

    [Theory]
    [InlineData(1, 3, 183, true)]
    [InlineData(1, 0, 183, false)]
    [InlineData(1, 73, 183, false)]
    [InlineData(1, 3, 29, false)]
    [InlineData(0, 3, 183, false)]
    public void Settings_NeedAVersion_AndValuesInRange(uint version, int hours, int days, bool valid)
    {
        Assert.Equal(valid, new UpdateSettingsValidator().Validate(new UpdateSettingsCommand(version, hours, days)).IsValid);
    }

    [Theory]
    [InlineData(1, 50, true)]
    [InlineData(0, 50, false)]
    [InlineData(1, 101, false)]
    public void History_PagesAreAtMost100(int page, int pageSize, bool valid)
    {
        Assert.Equal(valid, new ListHistoryValidator().Validate(new ListHistoryQuery(page, pageSize)).IsValid);
    }
}
