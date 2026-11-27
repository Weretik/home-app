import MonitorHeartOutlinedIcon from '@mui/icons-material/MonitorHeartOutlined';
import TelegramIcon from '@mui/icons-material/Telegram';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { locale } from '../../../platform/locale';
import { SectionCard } from './SectionCard';
import { SummarySwitcher } from './SummarySwitcher';
import { useSummaryParam } from './use-summary-param';

const today = new Intl.DateTimeFormat(locale, {
  weekday: 'long',
  day: 'numeric',
  month: 'long',
});

/**
 * Головна: one section's summary at a time, picked with the switcher (`?s=`). Today «Бюджет» and «Подарунки» have one; the
 * others join through `summary-sections.ts`. The service pages «Стан» and «Телеграм» are linked below for phones,
 * which have no side menu.
 */
export function HomePage() {
  const [active, setActive] = useSummaryParam();
  const Summary = active.Summary;

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <Box>
        <Typography variant="h4" component="h1">
          Головна
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
          gap: { xs: 2, md: 3 },
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
          title="Стан"
          caption="Чи працюють сервер і база"
        />
        <SectionCard
          to="/notifications"
          color="secondary"
          icon={TelegramIcon}
          title="Телеграм"
          caption="Бот: чати, розклад, надсилання"
        />
      </Box>
    </Box>
  );
}
