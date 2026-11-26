import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { Outlet } from 'react-router-dom';
import { SectionTabs } from './SectionTabs';

/** The Gifts section frame: the title, the tabs of its pages, and the open page below. */
export function GiftsSection() {
  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <Typography variant="h4" component="h1">
        Подарки
      </Typography>
      <SectionTabs />
      <Outlet />
    </Box>
  );
}
