import AddOutlinedIcon from '@mui/icons-material/AddOutlined';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import Chip from '@mui/material/Chip';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import Typography from '@mui/material/Typography';
import type { SpendingLimitItem } from '../budget-data/budget-api';
import { useExpenseCategories } from '../data/expenses-api';
import { formatAmount } from '../data/formatting';
import { limitPeriodLabels } from '../data/labels';
import { RowActions } from '../entry-dialogs/RowActions';
import type { RowHandlers } from '../entry-editing/use-entry-editor';

interface LimitsCardProps extends RowHandlers<SpendingLimitItem> {
  limits: SpendingLimitItem[];
  onAdd: () => void;
}

/** The spending limits: name, amount per month or quarter, the categories as chips; add, edit and delete. */
export function LimitsCard({
  limits,
  onAdd,
  onEdit,
  onDelete,
}: LimitsCardProps) {
  const categories = useExpenseCategories();
  const names = new Map(
    (categories.data ?? []).map((category) => [category.id, category.name]),
  );

  return (
    <Card component="section" aria-labelledby="limits-title">
      <Box
        sx={{
          p: { xs: 2, md: 3 },
          pb: { xs: 1, md: 1 },
          display: 'flex',
          flexWrap: 'wrap',
          alignItems: 'center',
          justifyContent: 'space-between',
          gap: 2,
        }}
      >
        <Box>
          <Typography
            id="limits-title"
            variant="subtitle1"
            component="h3"
            sx={{ fontWeight: 600 }}
          >
            Лимиты по категориям
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Сколько можно потратить на группу категорий; лимиты могут
            пересекаться.
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<AddOutlinedIcon />}
          onClick={onAdd}
        >
          Добавить лимит
        </Button>
      </Box>
      {limits.length === 0 ? (
        <Typography color="text.secondary" sx={{ px: { xs: 2, md: 3 }, pb: 3 }}>
          Лимитов пока нет. Добавьте первый, например «Еда» — 15 000 ₴ в месяц.
        </Typography>
      ) : (
        <List aria-label="Лимиты" sx={{ px: { md: 1 } }}>
          {limits.map((limit, index) => (
            <ListItem
              key={limit.id}
              divider={index < limits.length - 1}
              sx={{ alignItems: 'flex-start', gap: 2, py: 1.5 }}
            >
              <Box sx={{ flex: 1, minWidth: 0, display: 'grid', gap: 1 }}>
                <Typography sx={{ fontWeight: 600, overflowWrap: 'anywhere' }}>
                  {limit.name}
                </Typography>
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.75 }}>
                  {limit.categoryIds.map((id) => (
                    <Chip key={id} size="small" label={names.get(id) ?? '…'} />
                  ))}
                </Box>
              </Box>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                <Box sx={{ textAlign: 'right' }}>
                  <Typography
                    sx={{
                      fontWeight: 600,
                      whiteSpace: 'nowrap',
                      fontVariantNumeric: 'tabular-nums',
                    }}
                  >
                    {formatAmount(limit.amount)}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {limitPeriodLabels[limit.period]}
                  </Typography>
                </Box>
                <RowActions
                  label={`${limit.name}, ${formatAmount(limit.amount)}`}
                  onEdit={() => onEdit(limit)}
                  onDelete={() => onDelete(limit)}
                />
              </Box>
            </ListItem>
          ))}
        </List>
      )}
    </Card>
  );
}
