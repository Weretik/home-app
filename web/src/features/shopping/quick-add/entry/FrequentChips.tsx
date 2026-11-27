import Box from '@mui/material/Box';
import Chip from '@mui/material/Chip';
import { useFrequentItems } from '../../data/shopping-api';
import type { QuickAddController } from './use-quick-add';

/** ⭐ Часті: one tap adds the item with its usual department (R-8). Scrolls sideways on a phone. */
export function FrequentChips({ quickAdd }: { quickAdd: QuickAddController }) {
  const { data } = useFrequentItems();
  if (!data || data.length === 0) return null;

  return (
    <Box
      component="ul"
      aria-label="Часті"
      sx={{
        display: 'flex',
        gap: 1,
        overflowX: 'auto',
        m: 0,
        p: 0,
        pb: 0.5,
        listStyle: 'none',
        scrollbarWidth: 'thin',
      }}
    >
      {data.map((item) => (
        <Box component="li" key={item.name} sx={{ flex: 'none' }}>
          <Chip
            label={`⭐ ${item.name}`}
            variant="outlined"
            disabled={quickAdd.isPending}
            onClick={() =>
              quickAdd.submit(item.name, { departmentId: item.departmentId })
            }
            aria-label={`Додати ${item.name}`}
          />
        </Box>
      ))}
    </Box>
  );
}
