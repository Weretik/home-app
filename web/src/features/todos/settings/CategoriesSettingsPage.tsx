import AddOutlinedIcon from '@mui/icons-material/AddOutlined';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import List from '@mui/material/List';
import Skeleton from '@mui/material/Skeleton';
import Typography from '@mui/material/Typography';
import { useState } from 'react';
import { useCategories, type TodoCategoryItem } from '../data/categories-api';
import { CategoryDialog } from './CategoryDialog';
import { CategoryRow } from './CategoryRow';
import { DeleteCategoryDialog } from './DeleteCategoryDialog';

type Open =
  { mode: 'add' } | { mode: 'edit' | 'delete'; item: TodoCategoryItem } | null;

/** «Налаштування» of the section: the categories with emoji, name and active count; add, change, delete (FR-007). */
export function CategoriesSettingsPage() {
  const categories = useCategories();
  const [open, setOpen] = useState<Open>(null);
  const items = categories.data ?? [];
  // The open category as the list holds it now: after a 409 the list reloads, and a retry sends the new version.
  const latest = (item: TodoCategoryItem) =>
    items.find((category) => category.id === item.id) ?? item;

  return (
    <Card
      component="section"
      aria-labelledby="todo-categories-title"
      sx={{ minWidth: 0 }}
    >
      <Box
        sx={{
          p: 2,
          display: 'flex',
          flexWrap: 'wrap',
          alignItems: 'center',
          justifyContent: 'space-between',
          gap: 1,
        }}
      >
        <Typography
          id="todo-categories-title"
          variant="subtitle1"
          component="h2"
          sx={{ fontWeight: 600 }}
        >
          Категорії
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddOutlinedIcon />}
          onClick={() => setOpen({ mode: 'add' })}
        >
          Додати категорію
        </Button>
      </Box>
      {categories.isPending ? (
        <Box
          role="progressbar"
          aria-label="Завантажую категорії"
          sx={{ px: 2, pb: 2 }}
        >
          {[0, 1, 2].map((row) => (
            <Skeleton key={row} height={48} />
          ))}
        </Box>
      ) : categories.error ? (
        <Box sx={{ px: 2, pb: 2 }}>
          <Alert
            severity="error"
            action={
              <Button
                color="inherit"
                size="small"
                onClick={() => void categories.refetch()}
              >
                Повторити
              </Button>
            }
          >
            Не вдалося завантажити: {categories.error.message}
          </Alert>
        </Box>
      ) : items.length === 0 ? (
        <Typography color="text.secondary" sx={{ px: 2, pb: 2 }}>
          Категорій немає. Додайте першу — наприклад, «🏠 Дім».
        </Typography>
      ) : (
        <List
          disablePadding
          aria-label="Категорії справ"
          sx={{ borderTop: 1, borderColor: 'divider' }}
        >
          {items.map((item, index) => (
            <CategoryRow
              key={item.id}
              item={item}
              divider={index < items.length - 1}
              onEdit={() => setOpen({ mode: 'edit', item })}
              onDelete={() => setOpen({ mode: 'delete', item })}
            />
          ))}
        </List>
      )}
      {open?.mode === 'delete' ? (
        <DeleteCategoryDialog
          item={latest(open.item)}
          onClose={() => setOpen(null)}
        />
      ) : open ? (
        <CategoryDialog
          item={open.mode === 'edit' ? latest(open.item) : undefined}
          onClose={() => setOpen(null)}
        />
      ) : null}
    </Card>
  );
}
