import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { Outlet } from 'react-router-dom';
import { SectionTabs } from './SectionTabs';

/** The Todos section frame: the title, the tabs of its pages and the gear, and the open page below. */
export function TodosSection() {
  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <Typography variant="h4" component="h1">
        Справи
      </Typography>
      <SectionTabs />
      <Outlet />
    </Box>
  );
}
