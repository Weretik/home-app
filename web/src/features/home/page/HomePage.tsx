import MonitorHeartOutlinedIcon from '@mui/icons-material/MonitorHeartOutlined';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { SectionCard } from './SectionCard';
import { SummarySwitcher } from './SummarySwitcher';
import { useSummaryParam } from './use-summary-param';

const today = new Intl.DateTimeFormat('ru-RU', {
  weekday: 'long',
  day: 'numeric',
  month: 'long',
});

/**
 * Главная: one section's summary at a time, picked with the switcher (`?s=`). Today only «Бюджет» has one; the
 * others join through `summary-sections.ts`. The service page «Состояние» is linked below for phones, which have no
 * side menu.
 */
export function HomePage() {
  const [active, setActive] = useSummaryParam();
  const Summary = active.Summary;

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <Box>
        <Typography variant="h4" component="h1">
          Главная
        </Typography>
        <Typography
          color="text.secondary"
          sx={{ '&::first-letter': { textTransform: 'uppercase' } }}
        >
          {today.format(new Date())}
        </Typography>
      </Box>

      <SummarySwitcher value={active.key} onChange={setActive} />
      {Summary && <Summary />}

      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: {
            xs: '1fr',
            sm: '1fr 1fr',
            lg: 'repeat(3, 1fr)',
          },
        }}
      >
        <SectionCard
          to="/status"
          color="primary"
          icon={MonitorHeartOutlinedIcon}
          title="Состояние"
          caption="Работают ли сервер и база"
        />
      </Box>
    </Box>
  );
}
