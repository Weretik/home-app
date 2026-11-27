import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import Typography from '@mui/material/Typography';
import { Link } from 'react-router-dom';
import { forecastLines } from '../data/figure-lines';
import { formatExact } from '../data/money';
import type { MonthSummary } from '../data/summary-api';

const title = 'Прогноз';

/**
 * «Прогноз»: what the month will come to with the planned expenses — expected expenses, planned, the expected result
 * and what stays of the monthly limit — and a link to the month's plans.
 */
export function ForecastCard({ summary }: { summary: MonthSummary }) {
  const lines = forecastLines(summary);
  return (
    <Card component="section" aria-label={title} sx={{ p: { xs: 2, md: 2.5 } }}>
      <Box
        sx={{
          display: 'flex',
          flexWrap: 'wrap',
          alignItems: 'center',
          justifyContent: 'space-between',
          gap: 1,
        }}
      >
        <Typography variant="subtitle1" component="h2" sx={{ fontWeight: 600 }}>
          {title}
        </Typography>
        <Button
          size="small"
          component={Link}
          to={`/expenses/forecast?year=${summary.year}&month=${summary.month}`}
        >
          Відкрити план
        </Button>
      </Box>
      {summary.forecast.planned === 0 && (
        <Typography variant="body2" color="text.secondary">
          На місяць нічого не заплановано: прогноз дорівнює тому, що вже
          витрачено.
        </Typography>
      )}
      <List dense aria-label={title}>
        {lines.map((line) => (
          <ListItem
            key={line.key}
            disableGutters
            sx={{ display: 'flex', gap: 2, justifyContent: 'space-between' }}
          >
            <Typography sx={{ minWidth: 0 }}>{line.name}</Typography>
            <Typography
              sx={{
                whiteSpace: 'nowrap',
                fontVariantNumeric: 'tabular-nums',
                fontWeight: 600,
                color: line.tone ? `${line.tone}.main` : 'text.primary',
              }}
            >
              {formatExact(line.amount)}
            </Typography>
          </ListItem>
        ))}
      </List>
    </Card>
  );
}
