using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Expenses.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Limit amounts kept by the period they start in (ADR 0041, SDD 006): each current amount becomes one step from
    /// 2000-01-01, so nothing about earlier months changes on release. Expand only, so the previous release keeps
    /// working while this runs before the new code (rules.md "add first, remove later", ADR 0016): the old
    /// <c>amount</c> / <c>monthly_limit</c> columns stay (no longer mapped, given a default of 0 so the new code can
    /// insert), and the new columns keep the default <c>'[]'</c> so the previous release can still insert a limit (the
    /// new code reads <c>[]</c> as "no limit"). A follow-up migration after the release fills such rows from the old
    /// amount and drops the old columns (SDD 006 data model, task T13). <c>Down</c> puts back the amount in force in the
    /// current month.
    /// </summary>
    public partial class LimitAmountsByMonth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "amounts",
                schema: "expenses",
                table: "spending_limits",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "monthly_limits",
                schema: "expenses",
                table: "budget_settings",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.Sql(@"
UPDATE expenses.spending_limits
SET amounts = jsonb_build_array(jsonb_build_object('from', '2000-01-01', 'amount', amount));

UPDATE expenses.budget_settings
SET monthly_limits = jsonb_build_array(jsonb_build_object('from', '2000-01-01', 'amount', monthly_limit));

ALTER TABLE expenses.spending_limits ALTER COLUMN amount SET DEFAULT 0;
ALTER TABLE expenses.budget_settings ALTER COLUMN monthly_limit SET DEFAULT 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The amount in force in the current month: the last step that starts on or before it, else the first step.
            // A row still holding '[]' (inserted by the previous release) keeps the amount it was inserted with.
            migrationBuilder.Sql(@"
UPDATE expenses.spending_limits l
SET amount = COALESCE(
    (SELECT (s ->> 'amount')::numeric(12,2)
     FROM jsonb_array_elements(l.amounts) s
     WHERE (s ->> 'from')::date <= date_trunc('month', now())::date
     ORDER BY (s ->> 'from')::date DESC
     LIMIT 1),
    (l.amounts -> 0 ->> 'amount')::numeric(12,2))
WHERE jsonb_array_length(l.amounts) > 0;

UPDATE expenses.budget_settings b
SET monthly_limit = COALESCE(
    (SELECT (s ->> 'amount')::numeric(12,2)
     FROM jsonb_array_elements(b.monthly_limits) s
     WHERE (s ->> 'from')::date <= date_trunc('month', now())::date
     ORDER BY (s ->> 'from')::date DESC
     LIMIT 1),
    (b.monthly_limits -> 0 ->> 'amount')::numeric(12,2))
WHERE jsonb_array_length(b.monthly_limits) > 0;

ALTER TABLE expenses.spending_limits ALTER COLUMN amount DROP DEFAULT;
ALTER TABLE expenses.budget_settings ALTER COLUMN monthly_limit DROP DEFAULT;");

            migrationBuilder.DropColumn(
                name: "amounts",
                schema: "expenses",
                table: "spending_limits");

            migrationBuilder.DropColumn(
                name: "monthly_limits",
                schema: "expenses",
                table: "budget_settings");
        }
    }
}
