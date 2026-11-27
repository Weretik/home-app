import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import type { Theme } from '@mui/material/styles';
import useMediaQuery from '@mui/material/useMediaQuery';
import { useState } from 'react';
import { useEntryEditor } from '../entry-editing/use-entry-editor';
import { ListStatus } from '../month-view/ListStatus';
import { PageToolbar } from '../month-view/PageToolbar';
import {
  usePlannedExpenses,
  type PlannedExpenseItem,
} from '../planned-data/planned-api';
import { PlannedEditor } from '../planned-editing/PlannedEditor';
import { useMonthParam } from '../url-state/use-month-param';
import { PlannedList } from './PlannedList';
import { PlannedTable } from './PlannedTable';
import { PlannedTotal } from './PlannedTotal';
import { planLine } from './planned-rows';

/**
 * «Прогноз»: the expenses planned for the open month with their total «Заплановано»; each plan can be changed,
 * deleted or recorded as a real expense («Записати як витрату»). The plans count in the summary's forecast and limits.
 */
export function ForecastPage() {
  const [month] = useMonthParam();
  const query = usePlannedExpenses(month);
  const editor = useEntryEditor<PlannedExpenseItem>();
  const [recording, setRecording] = useState<PlannedExpenseItem | null>(null);
  const isComputer = useMediaQuery((theme: Theme) =>
    theme.breakpoints.up('md'),
  );

  const items = query.data?.items ?? [];
  const rows = {
    items,
    onEdit: editor.edit,
    onDelete: editor.remove,
    onRecord: setRecording,
  };

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <PageToolbar addLabel="Додати план" onAdd={editor.add}>
        <PlannedTotal
          plans={
            query.data && {
              totalAmount: query.data.totalAmount,
              count: items.length,
            }
          }
        />
      </PageToolbar>
      <Card>
        <ListStatus
          isPending={query.isPending}
          error={query.error}
          isEmpty={items.length === 0}
          scope="plan"
          loadingLabel="Завантажую план"
          onRetry={() => void query.refetch()}
        >
          {isComputer ? <PlannedTable {...rows} /> : <PlannedList {...rows} />}
        </ListStatus>
      </Card>
      <PlannedEditor
        editor={editor}
        recording={recording}
        onRecordClose={() => setRecording(null)}
        month={month}
        describe={planLine}
      />
    </Box>
  );
}
