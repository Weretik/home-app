using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gifts.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GiftPurchasePlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "buy_by",
                schema: "gifts",
                table: "gifts",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "giver",
                schema: "gifts",
                table: "gifts",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "gifts",
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
                name: "IX_outbox_messages_ClaimToken",
                schema: "gifts",
                table: "outbox_messages",
                column: "ClaimToken");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_ProcessedAtUtc_FailedAtUtc_NextAttemptAtUtc",
                schema: "gifts",
                table: "outbox_messages",
                columns: new[] { "ProcessedAtUtc", "FailedAtUtc", "NextAttemptAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "gifts");

            migrationBuilder.DropColumn(
                name: "buy_by",
                schema: "gifts",
                table: "gifts");

            migrationBuilder.DropColumn(
                name: "giver",
                schema: "gifts",
                table: "gifts");
        }
    }
}
