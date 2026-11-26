import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { Outlet } from 'react-router-dom';
import { MonthPicker } from './MonthPicker';
import { SectionTabs } from './SectionTabs';

/** The Expenses section frame: the title, the month, the tabs of its pages, and the open page below. */
export function ExpensesSection() {
  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <Box
        sx={{
          display: 'flex',
          flexWrap: 'wrap',
          alignItems: 'center',
          justifyContent: 'space-between',
          gap: 1,
        }}
      >
        <Typography variant="h4" component="h1">
          Расходы
        </Typography>
        <MonthPicker />
      </Box>
      <SectionTabs />
      <Outlet />
    </Box>
  );
}
