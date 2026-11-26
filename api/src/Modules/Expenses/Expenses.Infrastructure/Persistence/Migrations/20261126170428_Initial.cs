using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Expenses.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "expenses");

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "incomes",
                schema: "expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    person = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    import_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incomes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "expenses",
                schema: "expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    payer = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    payment_method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    import_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expenses", x => x.id);
                    table.ForeignKey(
                        name: "fk_expenses_categories",
                        column: x => x.category_id,
                        principalSchema: "expenses",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "expenses",
                table: "categories",
                columns: new[] { "id", "name", "sort_order" },
                values: new object[,]
                {
                    { new Guid("02d964e6-9360-5e7f-9685-6133d03031ef"), "Подарки", 100 },
                    { new Guid("08305dd6-28c2-59a1-9d98-0026828b4eee"), "Зоотовары", 120 },
                    { new Guid("12b7f2d2-0f0c-5d11-9691-86b3bc919ffe"), "Быт", 20 },
                    { new Guid("1ab0947d-0601-555e-8bc2-4b4996cbbbe7"), "Сдаваемая квартира", 140 },
                    { new Guid("2e1a44f6-fdf2-54ba-998b-ed490c80098d"), "Кафе, доставка, развлечения", 60 },
                    { new Guid("35252225-cd52-5360-b8a6-c1d16d8fe1e9"), "Личные Виталий", 30 },
                    { new Guid("35a6593b-5d73-5fcb-aea8-a2bb9f8e8eb2"), "Мебель и техника", 130 },
                    { new Guid("361658b1-d605-57e0-b6e6-37f5c8c80e7f"), "Здоровье", 70 },
                    { new Guid("77ad0660-c002-50f6-ad87-b04c1762cf13"), "Коммуналка", 50 },
                    { new Guid("86c6ddca-a991-5d9b-97b9-468b90f50c4c"), "Продукты", 10 },
                    { new Guid("a064005b-fd96-5454-b17a-8bf70ff694f2"), "Одежда", 80 },
                    { new Guid("dbd79e28-606e-5efb-bc73-f294ba6d0d5d"), "Уходовые средства и косметика", 110 },
                    { new Guid("e423ab6d-bafe-5a11-a1b0-fbfc88e4bb08"), "Личные Маша", 40 },
                    { new Guid("eec40aeb-c2a3-5aea-93e8-d4f4929c79aa"), "Ремонт", 90 }
                });

            migrationBuilder.CreateIndex(
                name: "ux_categories_name",
                schema: "expenses",
                table: "categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_expenses_category_id",
                schema: "expenses",
                table: "expenses",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_expenses_date",
                schema: "expenses",
                table: "expenses",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "ux_expenses_import_key",
                schema: "expenses",
                table: "expenses",
                column: "import_key",
                unique: true,
                filter: "import_key IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_incomes_date",
                schema: "expenses",
                table: "incomes",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "ux_incomes_import_key",
                schema: "expenses",
                table: "incomes",
                column: "import_key",
                unique: true,
                filter: "import_key IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "expenses",
                schema: "expenses");

            migrationBuilder.DropTable(
                name: "incomes",
                schema: "expenses");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "expenses");
        }
    }
}
