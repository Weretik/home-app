using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Todos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "todos");

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "todos",
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
                schema: "todos",
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
                name: "tasks",
                schema: "todos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    assignee = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    author = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    priority = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    import_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tasks", x => x.id);
                    table.ForeignKey(
                        name: "fk_tasks_categories",
                        column: x => x.category_id,
                        principalSchema: "todos",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "task_steps",
                schema: "todos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_done = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    task_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_steps", x => x.id);
                    table.ForeignKey(
                        name: "fk_task_steps_tasks",
                        column: x => x.task_id,
                        principalSchema: "todos",
                        principalTable: "tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "todos",
                table: "categories",
                columns: new[] { "id", "emoji", "name", "sort_order" },
                values: new object[,]
                {
                    { new Guid("0193b6a0-0000-7000-8000-000000000010"), "🏠", "Дом", 10 },
                    { new Guid("0193b6a0-0000-7000-8000-000000000020"), "🔨", "Ремонт", 20 },
                    { new Guid("0193b6a0-0000-7000-8000-000000000030"), "🩺", "Здоровье", 30 },
                    { new Guid("0193b6a0-0000-7000-8000-000000000040"), "💼", "Работа", 40 },
                    { new Guid("0193b6a0-0000-7000-8000-000000000050"), "💳", "Финансы", 50 },
                    { new Guid("0193b6a0-0000-7000-8000-000000000060"), "📄", "Документы", 60 },
                    { new Guid("0193b6a0-0000-7000-8000-000000000070"), "🛍️", "Покупки", 70 },
                    { new Guid("0193b6a0-0000-7000-8000-000000000080"), "🌴", "Отдых", 80 }
                });

            migrationBuilder.CreateIndex(
                name: "ix_task_steps_task_id_sort_order",
                schema: "todos",
                table: "task_steps",
                columns: new[] { "task_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_tasks_category_id",
                schema: "todos",
                table: "tasks",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_completed_at_utc",
                schema: "todos",
                table: "tasks",
                column: "completed_at_utc",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "ix_tasks_status_due_date",
                schema: "todos",
                table: "tasks",
                columns: new[] { "status", "due_date" });

            migrationBuilder.CreateIndex(
                name: "ux_tasks_import_key",
                schema: "todos",
                table: "tasks",
                column: "import_key",
                unique: true);

            // Category names are unique without regard to case (R-011); EF cannot index an expression.
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX ux_categories_lower_name ON todos.categories (lower(name));");

            // The title search is LIKE on lower(title). pg_trgm is a trusted extension, so the database owner can add
            // it; if the host still refuses, the search works without the index (the table is small).
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    CREATE EXTENSION IF NOT EXISTS pg_trgm;
                EXCEPTION WHEN insufficient_privilege OR feature_not_supported OR undefined_file THEN
                    RAISE NOTICE 'pg_trgm is not available; todos.tasks gets no trigram index.';
                END $$;
                """);
            migrationBuilder.Sql("""
                DO $$
                DECLARE trgm_schema text;
                BEGIN
                    SELECT n.nspname INTO trgm_schema
                    FROM pg_extension e JOIN pg_namespace n ON n.oid = e.extnamespace
                    WHERE e.extname = 'pg_trgm';
                    IF trgm_schema IS NOT NULL THEN
                        EXECUTE format(
                            'CREATE INDEX ix_tasks_lower_title_trgm ON todos.tasks USING gin (lower(title) %I.gin_trgm_ops)',
                            trgm_schema);
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The expression indexes go with their tables; pg_trgm stays, other schemas may use it.
            migrationBuilder.DropTable(
                name: "create_requests",
                schema: "todos");

            migrationBuilder.DropTable(
                name: "task_steps",
                schema: "todos");

            migrationBuilder.DropTable(
                name: "tasks",
                schema: "todos");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "todos");
        }
    }
}
