using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Notes.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "notes");

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "notes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    emoji = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "create_requests",
                schema: "notes",
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
                name: "notes",
                schema: "notes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    about = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    kind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    details = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    place = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_important = table.Column<bool>(type: "boolean", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notes", x => x.id);
                    table.ForeignKey(
                        name: "fk_notes_categories",
                        column: x => x.category_id,
                        principalSchema: "notes",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                schema: "notes",
                table: "categories",
                columns: new[] { "id", "emoji", "name", "sort_order" },
                values: new object[,]
                {
                    { new Guid("019ac3a0-0000-7000-8000-000000000010"), "🍲", "Їжа", 10 },
                    { new Guid("019ac3a0-0000-7000-8000-000000000020"), "🥤", "Напої", 20 },
                    { new Guid("019ac3a0-0000-7000-8000-000000000030"), "💐", "Квіти", 30 },
                    { new Guid("019ac3a0-0000-7000-8000-000000000040"), "💻", "Техніка", 40 },
                    { new Guid("019ac3a0-0000-7000-8000-000000000050"), "👕", "Одяг", 50 },
                    { new Guid("019ac3a0-0000-7000-8000-000000000060"), "🎬", "Дозвілля", 60 },
                    { new Guid("019ac3a0-0000-7000-8000-000000000070"), "✈️", "Подорожі", 70 },
                    { new Guid("019ac3a0-0000-7000-8000-000000000080"), "🩺", "Здоров'я", 80 },
                    { new Guid("019ac3a0-0000-7000-8000-000000000090"), "🏠", "Дім", 90 },
                    { new Guid("019ac3a0-0000-7000-8000-000000000100"), "📝", "Інше", 100 }
                });

            migrationBuilder.CreateIndex(
                name: "ix_notes_about_kind",
                schema: "notes",
                table: "notes",
                columns: new[] { "about", "kind" });

            migrationBuilder.CreateIndex(
                name: "ix_notes_category_id",
                schema: "notes",
                table: "notes",
                column: "category_id");

            // Category names are unique without regard to case (R-011); EF cannot index an expression. The search
            // has no trigram index: the host forbids pg_trgm and the table holds hundreds of rows (ADR 0039).
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX ux_categories_lower_name ON notes.categories (lower(name));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The expression index goes with its table.
            migrationBuilder.DropTable(
                name: "create_requests",
                schema: "notes");

            migrationBuilder.DropTable(
                name: "notes",
                schema: "notes");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "notes");
        }
    }
}
