using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Expenses.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Budget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "budget_settings",
                schema: "expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    monthly_limit = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    savings_goal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_budget_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "spending_limits",
                schema: "expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    category_ids = table.Column<List<Guid>>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spending_limits", x => x.id);
                });

            migrationBuilder.InsertData(
                schema: "expenses",
                table: "budget_settings",
                columns: new[] { "id", "monthly_limit", "savings_goal" },
                values: new object[] { new Guid("5b0f3a2e-1c4d-4e8a-9f6b-2d7c8e9a0b1c"), 50000m, 100000m });

            // The limits of the old «Бюджет» sheet (see BudgetSeed); later changes are made on the «Настройки» page.
            migrationBuilder.Sql(@"INSERT INTO expenses.spending_limits (id, name, period, amount, category_ids, sort_order) VALUES
('3f1d6c1e-6b1a-4b3e-9a51-0b6f6c0e1a01', 'Еда и быт', 'Month', 15000, '{86c6ddca-a991-5d9b-97b9-468b90f50c4c,12b7f2d2-0f0c-5d11-9691-86b3bc919ffe}', 10),
('3f1d6c1e-6b1a-4b3e-9a51-0b6f6c0e1a02', 'Личные Виталий', 'Month', 7000, '{35252225-cd52-5360-b8a6-c1d16d8fe1e9}', 20),
('3f1d6c1e-6b1a-4b3e-9a51-0b6f6c0e1a03', 'Личные Маша', 'Month', 13000, '{e423ab6d-bafe-5a11-a1b0-fbfc88e4bb08}', 30),
('3f1d6c1e-6b1a-4b3e-9a51-0b6f6c0e1a04', 'Кафе, доставка, развлечения', 'Month', 3000, '{2e1a44f6-fdf2-54ba-998b-ed490c80098d}', 40),
('3f1d6c1e-6b1a-4b3e-9a51-0b6f6c0e1a05', 'Уход и косметика', 'Quarter', 3000, '{dbd79e28-606e-5efb-bc73-f294ba6d0d5d}', 50),
('3f1d6c1e-6b1a-4b3e-9a51-0b6f6c0e1a06', 'Зоотовары', 'Quarter', 2500, '{08305dd6-28c2-59a1-9d98-0026828b4eee}', 60),
('3f1d6c1e-6b1a-4b3e-9a51-0b6f6c0e1a07', 'Другое', 'Month', 15000, '{77ad0660-c002-50f6-ad87-b04c1762cf13,2e1a44f6-fdf2-54ba-998b-ed490c80098d,361658b1-d605-57e0-b6e6-37f5c8c80e7f,a064005b-fd96-5454-b17a-8bf70ff694f2,eec40aeb-c2a3-5aea-93e8-d4f4929c79aa,02d964e6-9360-5e7f-9685-6133d03031ef,dbd79e28-606e-5efb-bc73-f294ba6d0d5d,08305dd6-28c2-59a1-9d98-0026828b4eee,35a6593b-5d73-5fcb-aea8-a2bb9f8e8eb2,1ab0947d-0601-555e-8bc2-4b4996cbbbe7}', 70);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "budget_settings",
                schema: "expenses");

            migrationBuilder.DropTable(
                name: "spending_limits",
                schema: "expenses");
        }
    }
}
