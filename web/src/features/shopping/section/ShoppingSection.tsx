import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { Outlet } from 'react-router-dom';
import { countItems } from '../list-page/list-groups';
import { useShoppingList } from '../data/shopping-api';
import { MeToggle } from './MeToggle';
import { SectionTabs } from './SectionTabs';

/** The Shopping section frame: title with the list's counts, «Я», the tabs and the open page. */
export function ShoppingSection() {
  const { data } = useShoppingList();
  const counts = data ? countItems(data.items) : null;

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
        <Box>
          <Typography variant="h4" component="h1">
            Покупки
          </Typography>
          {counts && (
            <Typography
              color="text.secondary"
              sx={{ fontVariantNumeric: 'tabular-nums' }}
            >
              Нужно {counts.needed}
              {counts.urgent > 0 && ` · срочно ${counts.urgent}`} · в корзине{' '}
              {counts.inCart}
            </Typography>
          )}
        </Box>
        <MeToggle />
      </Box>
      <SectionTabs />
      <Outlet />
    </Box>
  );
}
