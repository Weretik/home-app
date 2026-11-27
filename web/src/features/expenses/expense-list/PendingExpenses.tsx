import Card from '@mui/material/Card';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import Typography from '@mui/material/Typography';
import { useExpenseCategories } from '../data/expenses-api';
import { formatAmount, formatDay } from '../data/formatting';
import { usePendingExpenses } from '../data/writes-api';

/**
 * «Очікують мережі»: the expenses added offline, shown at once above the list until they are sent (spec
 * expenses/007, quality target OFF2). Nothing is shown when none waits.
 */
export function PendingExpenses() {
  const pending = usePendingExpenses();
  const categories = useExpenseCategories();
  if (pending.length === 0) return null;
  const categoryName = (id: string) =>
    categories.data?.find((category) => category.id === id)?.name ?? 'Витрата';

  return (
    <Card component="section" aria-labelledby="pending-expenses-title">
      <Typography
        id="pending-expenses-title"
        variant="subtitle1"
        sx={{ px: { xs: 2, md: 3 }, pt: { xs: 2, md: 3 } }}
      >
        Очікують мережі
      </Typography>
      <Typography
        variant="body2"
        color="text.secondary"
        sx={{ px: { xs: 2, md: 3 } }}
      >
        Збережено на цьому пристрої. Надішлеться, коли з’явиться мережа.
      </Typography>
      <List aria-label="Витрати, що очікують мережі">
        {pending.map(({ key, body }) => (
          <ListItem key={key} sx={{ px: { xs: 2, md: 3 } }}>
            <ListItemText
              primary={`${categoryName(body.categoryId)} · ${formatAmount(body.amount)}`}
              secondary={formatDay(body.date)}
            />
          </ListItem>
        ))}
      </List>
    </Card>
  );
}
