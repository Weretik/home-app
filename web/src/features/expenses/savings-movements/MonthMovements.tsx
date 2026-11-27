import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import {
  useSavingsMovements,
  type SavingsMovementItem,
} from '../data/savings-api';
import type { Month } from '../data/month';
import { useEntryEditor } from '../entry-editing/use-entry-editor';
import { PagedListCard } from '../month-view/PagedListCard';
import { PageToolbar } from '../month-view/PageToolbar';
import { SavingsEditor } from '../savings-editing/SavingsEditor';
import { MovementList } from './MovementList';
import { MovementTable } from './MovementTable';

/** «Операції за місяць»: the open month's movements of the jar, page by page, with add, edit and delete. */
export function MonthMovements({ month }: { month: Month }) {
  const movements = useSavingsMovements(month);
  const editor = useEntryEditor<SavingsMovementItem>();
  const rows = { onEdit: editor.edit, onDelete: editor.remove };

  return (
    <Box sx={{ display: 'grid', gap: 2, minWidth: 0 }}>
      <PageToolbar addLabel="Додати" onAdd={editor.add}>
        <Typography variant="subtitle1" component="h2" sx={{ fontWeight: 600 }}>
          Операції за місяць
        </Typography>
      </PageToolbar>
      <PagedListCard query={movements} loadingLabel="Завантажую скарбничку">
        {(items, isComputer) =>
          isComputer ? (
            <MovementTable items={items} {...rows} />
          ) : (
            <MovementList items={items} {...rows} />
          )
        }
      </PagedListCard>
      <SavingsEditor editor={editor} month={month} />
    </Box>
  );
}
