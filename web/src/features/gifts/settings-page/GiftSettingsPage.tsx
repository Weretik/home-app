import Box from '@mui/material/Box';
import { CategoriesCard } from './CategoriesCard';
import { DetailsCard } from './DetailsCard';

/** «Настройки» of the Gifts section (the gear next to the tabs): the «нюансы» and the categories. */
export function GiftSettingsPage() {
  return (
    <Box
      sx={{
        display: 'grid',
        gap: { xs: 2, md: 3 },
        gridTemplateColumns: { xs: '1fr', lg: '3fr 2fr' },
        alignItems: 'start',
      }}
    >
      <DetailsCard />
      <CategoriesCard />
    </Box>
  );
}
