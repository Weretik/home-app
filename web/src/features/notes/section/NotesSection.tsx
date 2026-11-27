import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { Outlet } from 'react-router-dom';
import { NoteTabs } from './NoteTabs';

/** The «Про нас» section frame: the title, the person tabs and the gear, and the open page below (FR-001). */
export function NotesSection() {
  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <Typography variant="h4" component="h1">
        Про нас
      </Typography>
      <NoteTabs />
      <Outlet />
    </Box>
  );
}
