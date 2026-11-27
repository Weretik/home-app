import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import { Link } from 'react-router-dom';
import { recipientLabels, recipientOrder } from '../data/labels';
import type { GiftsSummaryView } from '../data/summary-api';
import { PersonAvatar } from '../persons/PersonAvatar';
import { SummaryCard } from './SummaryCard';

/** «Нюанси» read-only: sizes and details per person; they are edited only in the Gifts settings. */
export function DetailsCard({ summary }: { summary: GiftsSummaryView }) {
  const groups = recipientOrder
    .map((recipient) => ({
      recipient,
      items: summary.details.filter((d) => d.recipient === recipient),
    }))
    .filter((group) => group.items.length > 0);

  return (
    <SummaryCard
      title="Нюанси"
      action={
        <Button component={Link} to="/gifts/settings" size="small">
          Змінити
        </Button>
      }
    >
      {groups.length === 0 && (
        <Typography color="text.secondary">
          Нюансів поки немає. Розміри та інші деталі додаються в налаштуваннях
          подарунків.
        </Typography>
      )}
      {groups.map((group) => (
        <Box key={group.recipient}>
          <Typography
            variant="subtitle1"
            component="h3"
            sx={{
              fontWeight: 600,
              display: 'flex',
              alignItems: 'center',
              gap: 1,
              mb: 0.5,
            }}
          >
            <PersonAvatar recipient={group.recipient} size={24} />
            {recipientLabels[group.recipient]}
          </Typography>
          <Box
            component="dl"
            sx={{
              m: 0,
              display: 'grid',
              gridTemplateColumns: 'auto 1fr',
              columnGap: 2,
              rowGap: 0.5,
            }}
          >
            {group.items.map((detail) => (
              <Box key={detail.id} sx={{ display: 'contents' }}>
                <Typography component="dt">{detail.label}</Typography>
                <Typography
                  component="dd"
                  sx={{ m: 0, fontWeight: 600, overflowWrap: 'anywhere' }}
                >
                  {detail.value}
                </Typography>
              </Box>
            ))}
          </Box>
        </Box>
      ))}
    </SummaryCard>
  );
}
