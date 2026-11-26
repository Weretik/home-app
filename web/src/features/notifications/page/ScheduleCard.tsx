import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import type { NotificationScheduleItem } from '../data/notifications-api';
import { InfoCard } from './InfoCard';

/** «Расписание» (FR-005): what the bot sends and when, as the API words it (Kyiv time). */
export function ScheduleCard({
  schedule,
}: {
  schedule: NotificationScheduleItem[];
}) {
  return (
    <InfoCard title="Расписание" caption="Время киевское">
      <Box component="ul" sx={{ m: 0, p: 0, listStyle: 'none' }}>
        {schedule.map((item, index) => (
          <Box
            component="li"
            key={item.kind}
            sx={{
              py: 1,
              borderTop: index ? 1 : 0,
              borderColor: 'divider',
            }}
          >
            <Typography sx={{ fontWeight: 500 }}>{item.title}</Typography>
            <Typography variant="body2" color="text.secondary">
              {item.when}
            </Typography>
          </Box>
        ))}
      </Box>
    </InfoCard>
  );
}
