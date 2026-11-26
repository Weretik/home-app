using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Todos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TaskSourceAndOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "source_id",
                schema: "todos",
                table: "tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source_kind",
                schema: "todos",
                table: "tasks",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "todos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    NextAttemptAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClaimToken = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_tasks_source",
                schema: "todos",
                table: "tasks",
                columns: new[] { "source_kind", "source_id" },
                unique: true,
                filter: "source_kind IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_ClaimToken",
                schema: "todos",
                table: "outbox_messages",
                column: "ClaimToken");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_ProcessedAtUtc_FailedAtUtc_NextAttemptAtUtc",
                schema: "todos",
                table: "outbox_messages",
                columns: new[] { "ProcessedAtUtc", "FailedAtUtc", "NextAttemptAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "todos");

            migrationBuilder.DropIndex(
                name: "ux_tasks_source",
                schema: "todos",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "source_id",
                schema: "todos",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "source_kind",
                schema: "todos",
                table: "tasks");
        }
    }
}
