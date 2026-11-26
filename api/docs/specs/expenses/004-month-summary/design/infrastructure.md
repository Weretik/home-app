# Month summary — infrastructure design (as implemented)

## Queries

All `AsNoTracking`, grouped in SQL:

1. incomes of the month grouped by person;
2. expenses of the month grouped by payer;
3. the settings row;
4. all limits ordered;
5. expenses of the quarter around the month grouped by category, year and month (covers the month and every limit
   period);
6. jar net of the month and jar balance up to the month's end (two sums);
7. category names of the month's categories.

Limit usage and the top five are then assembled in memory from query 5. No query runs per limit.

## Migration and deployment

None: nothing is stored.
