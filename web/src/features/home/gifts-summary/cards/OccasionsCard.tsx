import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { formatDay } from '../data/formatting';
import { recipientLabels } from '../data/labels';
import type { GiftsSummaryView } from '../data/summary-api';
import { SummaryCard } from './SummaryCard';

/** «Ближайшие поводы»: wanted gifts with an occasion from today, the nearest first. */
export function OccasionsCard({ summary }: { summary: GiftsSummaryView }) {
  if (summary.upcoming.length === 0) return null;

  return (
    <SummaryCard title="Ближайшие поводы">
      <Box
        component="ul"
        sx={{ m: 0, p: 0, listStyle: 'none', display: 'grid', gap: 1.25 }}
      >
        {summary.upcoming.map((item) => (
          <Box component="li" key={item.id}>
            <Typography
              variant="body2"
              color="primary"
              sx={{ fontWeight: 600 }}
            >
              {formatDay(item.occasionDate)}
            </Typography>
            <Typography sx={{ overflowWrap: 'anywhere' }}>
              {item.name}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {[recipientLabels[item.recipient], item.occasion]
                .filter(Boolean)
                .join(' · ')}
            </Typography>
          </Box>
        ))}
      </Box>
    </SummaryCard>
  );
}
