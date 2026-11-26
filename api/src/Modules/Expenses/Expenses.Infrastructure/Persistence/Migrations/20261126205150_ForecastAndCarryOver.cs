using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Expenses.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ForecastAndCarryOver : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "outside_limits",
                schema: "expenses",
                table: "expenses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "limits_from",
                schema: "expenses",
                table: "budget_settings",
                type: "date",
                nullable: false,
                // The start of the old sheet's carry-over (BudgetSettings.SheetLimitsFrom); the seeded row gets it below.
                defaultValue: new DateOnly(2026, 11, 1));

            migrationBuilder.CreateTable(
                name: "planned_expenses",
                schema: "expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    month = table.Column<DateOnly>(type: "date", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payer = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planned_expenses", x => x.id);
                    table.ForeignKey(
                        name: "fk_planned_expenses_categories",
                        column: x => x.category_id,
                        principalSchema: "expenses",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                schema: "expenses",
                table: "budget_settings",
                keyColumn: "id",
                keyValue: new Guid("5b0f3a2e-1c4d-4e8a-9f6b-2d7c8e9a0b1c"),
                column: "limits_from",
                value: new DateOnly(2026, 11, 1));

            migrationBuilder.CreateIndex(
                name: "ix_planned_expenses_category_id",
                schema: "expenses",
                table: "planned_expenses",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_planned_expenses_month",
                schema: "expenses",
                table: "planned_expenses",
                column: "month");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "planned_expenses",
                schema: "expenses");

            migrationBuilder.DropColumn(
                name: "outside_limits",
                schema: "expenses",
                table: "expenses");

            migrationBuilder.DropColumn(
                name: "limits_from",
                schema: "expenses",
                table: "budget_settings");
        }
    }
}
