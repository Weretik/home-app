import OpenInNewOutlinedIcon from '@mui/icons-material/OpenInNewOutlined';
import Box from '@mui/material/Box';
import IconButton from '@mui/material/IconButton';
import Tooltip from '@mui/material/Tooltip';
import Typography from '@mui/material/Typography';
import { formatPrice } from '../data/formatting';
import { recipientLabels } from '../data/labels';
import type { GiftsSummaryView } from '../data/summary-api';
import { SummaryCard } from './SummaryCard';

/** «🔥 Очень хочу»: up to five most wanted gifts per person, with price and link. */
export function TopWishesCard({ summary }: { summary: GiftsSummaryView }) {
  const groups = summary.recipients.filter((item) => item.topWanted.length > 0);
  if (groups.length === 0) return null;

  return (
    <SummaryCard title="🔥 Очень хочу">
      {groups.map((group) => (
        <Box key={group.recipient}>
          <Typography variant="subtitle2" component="h3" color="text.secondary">
            {recipientLabels[group.recipient]}
          </Typography>
          <Box component="ul" sx={{ m: 0, p: 0, listStyle: 'none' }}>
            {group.topWanted.map((gift) => (
              <Box
                component="li"
                key={gift.id}
                sx={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: 1,
                  minHeight: 36,
                }}
              >
                <Typography
                  sx={{ flex: 1, minWidth: 0, overflowWrap: 'anywhere' }}
                >
                  {gift.name}
                </Typography>
                <Typography
                  variant="body2"
                  color={gift.price == null ? 'text.secondary' : 'text.primary'}
                  sx={{
                    whiteSpace: 'nowrap',
                    fontVariantNumeric: 'tabular-nums',
                  }}
                >
                  {formatPrice(gift.price)}
                </Typography>
                {gift.link && (
                  <Tooltip title="Открыть ссылку">
                    <IconButton
                      size="small"
                      href={gift.link}
                      target="_blank"
                      rel="noopener noreferrer"
                      aria-label={`Открыть ссылку: ${gift.name}`}
                    >
                      <OpenInNewOutlinedIcon fontSize="small" />
                    </IconButton>
                  </Tooltip>
                )}
              </Box>
            ))}
          </Box>
        </Box>
      ))}
    </SummaryCard>
  );
}
