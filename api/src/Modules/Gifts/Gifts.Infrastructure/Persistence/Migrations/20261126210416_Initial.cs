using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Gifts.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "gifts");

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "gifts",
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
                name: "create_requests",
                schema: "gifts",
                columns: table => new
                {
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_create_requests", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "details",
                schema: "gifts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    value = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_details", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "gifts",
                schema: "gifts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    priority = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    link = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    gifted_on = table.Column<DateOnly>(type: "date", nullable: true),
                    occasion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    occasion_date = table.Column<DateOnly>(type: "date", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gifts", x => x.id);
                    table.ForeignKey(
                        name: "fk_gifts_categories",
                        column: x => x.category_id,
                        principalSchema: "gifts",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "gift_variants",
                schema: "gifts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    link = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    gift_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gift_variants", x => x.id);
                    table.ForeignKey(
                        name: "fk_gift_variants_gifts",
                        column: x => x.gift_id,
                        principalSchema: "gifts",
                        principalTable: "gifts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "gifts",
                table: "categories",
                columns: new[] { "id", "name", "sort_order" },
                values: new object[,]
                {
                    { new Guid("126cde36-f757-558b-9179-e3af435fd0f2"), "Хобби и игры", 100 },
                    { new Guid("27851329-987a-5bc7-962e-bdea4b943d0c"), "Кухня", 30 },
                    { new Guid("40572c1e-f889-5f62-977f-4af1d43581ea"), "Ванная", 50 },
                    { new Guid("9ae4dce6-f172-5f74-a3f6-4979bf0dafbd"), "Спорт", 90 },
                    { new Guid("9dba6a43-1934-58e9-82d6-93a2fe5ca7e6"), "Другое", 110 },
                    { new Guid("a1b9482e-bc3b-53cd-8232-0dbe2b4aa249"), "Техника", 10 },
                    { new Guid("b5691e11-6405-5690-bba9-43e92fef7366"), "Украшения", 70 },
                    { new Guid("d5e823ec-44f6-58da-945a-9d4db1e93133"), "Уход и здоровье", 80 },
                    { new Guid("dadca160-ff2a-5a99-a27d-c743a38395bb"), "Мебель", 40 },
                    { new Guid("dcd4bc3e-5b21-5200-a4ac-2fb08317fa2f"), "Одежда и аксессуары", 60 },
                    { new Guid("f667a762-dff7-5683-8e31-e75127050717"), "Дом и уют", 20 }
                });

            migrationBuilder.CreateIndex(
                name: "ux_categories_name",
                schema: "gifts",
                table: "categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_details_recipient_sort_order",
                schema: "gifts",
                table: "details",
                columns: new[] { "recipient", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_gift_variants_gift_id",
                schema: "gifts",
                table: "gift_variants",
                column: "gift_id");

            migrationBuilder.CreateIndex(
                name: "ix_gifts_category_id",
                schema: "gifts",
                table: "gifts",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_gifts_status",
                schema: "gifts",
                table: "gifts",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "create_requests",
                schema: "gifts");

            migrationBuilder.DropTable(
                name: "details",
                schema: "gifts");

            migrationBuilder.DropTable(
                name: "gift_variants",
                schema: "gifts");

            migrationBuilder.DropTable(
                name: "gifts",
                schema: "gifts");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "gifts");
        }
    }
}
