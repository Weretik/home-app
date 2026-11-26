using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Expenses.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LimitSortOrderUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Two limits added at the same moment could share a place before this index; renumber them in their
            // current order (sort_order, then name) so the index can be created. No-op when the places are distinct.
            migrationBuilder.Sql(@"
UPDATE expenses.spending_limits AS l
SET sort_order = ordered.place * 10
FROM (SELECT id, row_number() OVER (ORDER BY sort_order, name, id) AS place FROM expenses.spending_limits) AS ordered
WHERE l.id = ordered.id
  AND EXISTS (SELECT 1 FROM expenses.spending_limits GROUP BY sort_order HAVING count(*) > 1);");

            migrationBuilder.CreateIndex(
                name: "ux_spending_limits_sort_order",
                schema: "expenses",
                table: "spending_limits",
                column: "sort_order",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_spending_limits_sort_order",
                schema: "expenses",
                table: "spending_limits");
        }
    }
}
