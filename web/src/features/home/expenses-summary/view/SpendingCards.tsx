import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import Typography from '@mui/material/Typography';
import { payerLabels } from '../data/labels';
import { formatExact, percentOf } from '../data/money';
import type { MonthSummary, Payer } from '../data/summary-api';

const payers: Payer[] = ['Vitaliy', 'Maria', 'Shared'];
const number = { whiteSpace: 'nowrap', fontVariantNumeric: 'tabular-nums' };

interface Line {
  key: string;
  name: string;
  amount: number;
  caption?: string;
}

/** A small card with a title and name / amount lines. */
function AmountsCard({
  title,
  lines,
  empty,
}: {
  title: string;
  lines: Line[];
  empty: string;
}) {
  return (
    <Card component="section" aria-label={title} sx={{ p: { xs: 2, md: 2.5 } }}>
      <Typography variant="subtitle1" component="h2" sx={{ fontWeight: 600 }}>
        {title}
      </Typography>
      {lines.length === 0 ? (
        <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
          {empty}
        </Typography>
      ) : (
        <List dense aria-label={title}>
          {lines.map((line) => (
            <ListItem
              key={line.key}
              disableGutters
              sx={{ display: 'flex', gap: 2, justifyContent: 'space-between' }}
            >
              <Box sx={{ minWidth: 0 }}>
                <Typography>{line.name}</Typography>
                {line.caption && (
                  <Typography variant="body2" color="text.secondary">
                    {line.caption}
                  </Typography>
                )}
              </Box>
              <Typography sx={{ ...number, fontWeight: 600 }}>
                {formatExact(line.amount)}
              </Typography>
            </ListItem>
          ))}
        </List>
      )}
    </Card>
  );
}

/** «Кто сколько потратил» and «Топ-5 категорий» side by side from `sm`. */
export function SpendingCards({ summary }: { summary: MonthSummary }) {
  const spentBy = (payer: Payer) =>
    summary.expensesByPayer.find((p) => p.payer === payer)?.amount ?? 0;
  const whoLines =
    summary.expenses > 0
      ? payers.map((payer) => ({
          key: payer,
          name: payerLabels[payer],
          amount: spentBy(payer),
          caption: `${percentOf(spentBy(payer), summary.expenses)}% расходов`,
        }))
      : [];
  const topLines = summary.topCategories.slice(0, 5).map((category) => ({
    key: category.categoryId,
    name: category.name,
    amount: category.amount,
  }));

  return (
    <Box
      sx={{
        display: 'grid',
        gap: { xs: 2, md: 3 },
        gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' },
      }}
    >
      <AmountsCard
        title="Кто сколько потратил"
        lines={whoLines}
        empty="Расходов за месяц нет."
      />
      <AmountsCard
        title="Топ-5 категорий"
        lines={topLines}
        empty="Расходов за месяц нет."
      />
    </Box>
  );
}
