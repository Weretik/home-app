import AddOutlinedIcon from '@mui/icons-material/AddOutlined';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import List from '@mui/material/List';
import Typography from '@mui/material/Typography';
import type { SpendingLimitItem } from '../budget-data/budget-api';
import { useExpenseCategories } from '../data/expenses-api';
import type { RowHandlers } from '../entry-editing/use-entry-editor';
import { LimitRow } from './LimitRow';

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
            Ліміти за категоріями
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Скільки можна витратити на групу категорій; ліміти можуть
            перетинатися.
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<AddOutlinedIcon />}
          onClick={onAdd}
        >
          Додати ліміт
        </Button>
      </Box>
      {limits.length === 0 ? (
        <Typography color="text.secondary" sx={{ px: { xs: 2, md: 3 }, pb: 3 }}>
          Лімітів поки немає. Додайте перший, наприклад «Їжа» — 15 000 ₴ на
          місяць.
        </Typography>
      ) : (
        <List aria-label="Ліміти" sx={{ px: { md: 1 } }}>
          {limits.map((limit, index) => (
            <LimitRow
              key={limit.id}
              limit={limit}
              categoryNames={names}
              divider={index < limits.length - 1}
              onEdit={onEdit}
              onDelete={onDelete}
            />
          ))}
        </List>
      )}
    </Card>
  );
}
