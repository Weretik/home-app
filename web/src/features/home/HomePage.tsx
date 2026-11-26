import CardGiftcardOutlinedIcon from '@mui/icons-material/CardGiftcardOutlined';
import ChecklistOutlinedIcon from '@mui/icons-material/ChecklistOutlined';
import MonitorHeartOutlinedIcon from '@mui/icons-material/MonitorHeartOutlined';
import ReceiptLongOutlinedIcon from '@mui/icons-material/ReceiptLongOutlined';
import ShoppingCartOutlinedIcon from '@mui/icons-material/ShoppingCartOutlined';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { HighlightCard } from './HighlightCard';
import { SectionCard } from './SectionCard';

const today = new Intl.DateTimeFormat('ru-RU', {
  weekday: 'long',
  day: 'numeric',
  month: 'long',
});

/**
 * The dashboard. Each card links to its section; the numbers arrive when the sections get their data.
 * Until then the cards say what will be there.
 */
export function HomePage() {
  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 } }}>
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

      <Box
        sx={{
          display: 'grid',
          gap: { xs: 2, md: 3 },
          gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' },
        }}
      >
        <HighlightCard
          to="/expenses"
          color="primary"
          icon={ReceiptLongOutlinedIcon}
          value="Скоро"
          title="Расходы за месяц"
          caption="Сумма и кто сколько потратил"
        />
        <HighlightCard
          to="/todos"
          color="secondary"
          icon={ChecklistOutlinedIcon}
          value="Скоро"
          title="Дела на сегодня"
          caption="Маша, Виталий и общие"
        />
      </Box>

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
          to="/shopping"
          color="primary"
          icon={ShoppingCartOutlinedIcon}
          title="Покупки"
          caption="Общий список, что купить"
        />
        <SectionCard
          to="/gifts"
          color="secondary"
          icon={CardGiftcardOutlinedIcon}
          title="Подарки"
          caption="Список желаний и идеи"
        />
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
