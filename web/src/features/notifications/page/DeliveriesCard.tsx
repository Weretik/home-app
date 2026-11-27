import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import {
  audienceLabels,
  channelLabels,
  deliveryTitle,
  formatKyivTime,
} from '../data/labels';
import type { NotificationsStatus } from '../data/notifications-api';
import { InfoCard } from './InfoCard';

/**
 * «Останні надсилання» (FR-006, notifications 002 FR-108): what was planned, on which channel, to whom, and whether it
 * went out; hidden when there is nothing.
 */
export function DeliveriesCard({ status }: { status: NotificationsStatus }) {
  if (status.deliveries.length === 0) return null;
  return (
    <InfoCard title="Останні надсилання">
      <Box component="ul" sx={{ m: 0, p: 0, listStyle: 'none' }}>
        {status.deliveries.map((delivery, index) => (
          <Box
            component="li"
            key={delivery.key}
            sx={{
              py: 1,
              borderTop: index ? 1 : 0,
              borderColor: 'divider',
              display: 'flex',
              flexWrap: 'wrap',
              justifyContent: 'space-between',
              columnGap: 2,
            }}
          >
            <Box sx={{ minWidth: 0 }}>
              <Typography sx={{ fontWeight: 500 }}>
                {deliveryTitle(delivery.kind, status.schedule)}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {channelLabels[delivery.channel]} ·{' '}
                {audienceLabels[delivery.audience]} ·{' '}
                {formatKyivTime(delivery.createdAtUtc)}
              </Typography>
            </Box>
            <Typography
              variant="body2"
              color={delivery.sentAtUtc ? 'text.secondary' : 'warning.main'}
              sx={{ alignSelf: 'center' }}
            >
              {delivery.sentAtUtc
                ? `надіслано ${formatKyivTime(delivery.sentAtUtc)}`
                : 'у черзі'}
            </Typography>
          </Box>
        ))}
      </Box>
    </InfoCard>
  );
}
