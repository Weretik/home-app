import AddOutlinedIcon from '@mui/icons-material/AddOutlined';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import type { Theme } from '@mui/material/styles';
import Typography from '@mui/material/Typography';
import useMediaQuery from '@mui/material/useMediaQuery';
import {
  useSavingsMovements,
  useSavingsOverview,
  type SavingsMovementItem,
} from '../data/savings-api';
import { useEntryEditor } from '../entry-editing/use-entry-editor';
import { ListStatus } from '../month-view/ListStatus';
import { LoadMoreButton } from '../month-view/LoadMoreButton';
import { SavingsEditor } from '../savings-editing/SavingsEditor';
import { useMonthParam } from '../url-state/use-month-param';
import { BalanceCard } from './BalanceCard';
import { MovementList } from './MovementList';
import { MovementTable } from './MovementTable';
import { SavingsMonthsCard } from './SavingsMonthsCard';

/** «Копилка»: what is in the jar now, the open month's movements with add / edit / delete, and every month's sums. */
export function SavingsPage() {
  const [month, setMonth] = useMonthParam();
  const movements = useSavingsMovements(month);
  const overview = useSavingsOverview();
  const editor = useEntryEditor<SavingsMovementItem>();
  const isComputer = useMediaQuery((theme: Theme) =>
    theme.breakpoints.up('md'),
  );

  const pages = movements.data?.pages ?? [];
  const items = pages.flatMap((page) => page.items);
  const first = pages[0];

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <BalanceCard
        currentBalance={overview.data?.currentBalance ?? first?.currentBalance}
        month={first}
      />
      <Box
        sx={{
          display: 'grid',
          gap: { xs: 2, md: 3 },
          gridTemplateColumns: {
            xs: 'minmax(0, 1fr)',
            lg: 'minmax(0, 2fr) minmax(0, 1fr)',
          },
          alignItems: 'start',
        }}
      >
        <Box sx={{ display: 'grid', gap: 2, minWidth: 0 }}>
          <Box
            sx={{
              display: 'flex',
              flexWrap: 'wrap',
              alignItems: 'center',
              justifyContent: 'space-between',
              gap: 2,
            }}
          >
            <Typography
              variant="subtitle1"
              component="h2"
              sx={{ fontWeight: 600 }}
            >
              Движения за месяц
            </Typography>
            <Button
              variant="contained"
              startIcon={<AddOutlinedIcon />}
              onClick={editor.add}
            >
              Добавить
            </Button>
          </Box>
          <Card>
            <ListStatus
              isPending={movements.isPending}
              error={movements.error}
              isEmpty={items.length === 0}
              isFiltered={false}
              loadingLabel="Загружаю копилку"
              onRetry={() => void movements.refetch()}
              onResetFilters={() => {}}
            >
              {isComputer ? (
                <MovementTable
                  items={items}
                  onEdit={editor.edit}
                  onDelete={editor.remove}
                />
              ) : (
                <MovementList
                  items={items}
                  onEdit={editor.edit}
                  onDelete={editor.remove}
                />
              )}
              {movements.hasNextPage && first && (
                <LoadMoreButton
                  shown={items.length}
                  total={first.totalCount}
                  isLoading={movements.isFetchingNextPage}
                  onClick={() => void movements.fetchNextPage()}
                />
              )}
            </ListStatus>
          </Card>
        </Box>
        <SavingsMonthsCard
          months={overview.data?.months}
          error={overview.error}
          open={month}
          onOpen={setMonth}
          onRetry={() => void overview.refetch()}
        />
      </Box>
      <SavingsEditor editor={editor} month={month} />
    </Box>
  );
}
