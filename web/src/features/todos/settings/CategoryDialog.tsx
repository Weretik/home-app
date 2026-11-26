import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import TextField from '@mui/material/TextField';
import { useId, useState, type FormEvent } from 'react';
import type { TodoCategoryItem } from '../data/categories-api';
import { describeWriteError, firstMessages } from '../writes/failures-api';
import { useSaveCategory } from '../writes/category-writes-api';
import {
  categoryFormValues,
  toCategoryRequest,
  validateCategory,
} from './category-form';

/** Adds a category or changes its name and emoji (FR-007). */
export function CategoryDialog({
  item,
  onClose,
}: {
  item?: TodoCategoryItem;
  onClose: () => void;
}) {
  const titleId = useId();
  const save = useSaveCategory();
  const [values, setValues] = useState(() => categoryFormValues(item));
  const [checked, setChecked] = useState<Record<string, string | undefined>>(
    {},
  );
  const failure = save.error
    ? describeWriteError(save.error, 'сохранить', 'category')
    : undefined;
  const errors = { ...firstMessages(failure), ...stripEmpty(checked) };

  const submit = (event: FormEvent) => {
    event.preventDefault();
    const found = validateCategory(values);
    setChecked(found);
    if (Object.values(found).some(Boolean) || save.isPending) return;
    save.mutate(
      {
        body: toCategoryRequest(values),
        edit: item && { id: item.id, version: item.version },
      },
      { onSuccess: onClose },
    );
  };

  const field = (name: 'name' | 'emoji') => ({
    value: values[name],
    onChange: (event: { target: { value: string } }) => {
      setValues((current) => ({ ...current, [name]: event.target.value }));
      setChecked((current) => ({ ...current, [name]: undefined }));
    },
    error: Boolean(errors[name]),
    helperText: errors[name],
  });

  return (
    <Dialog
      open
      fullWidth
      maxWidth="xs"
      onClose={save.isPending ? undefined : onClose}
      aria-labelledby={titleId}
    >
      <Box component="form" noValidate onSubmit={submit}>
        <DialogTitle id={titleId}>
          {item ? 'Изменить категорию' : 'Новая категория'}
        </DialogTitle>
        <DialogContent
          sx={{
            display: 'grid',
            gap: 2,
            '.MuiDialogTitle-root + &': { pt: 1 },
          }}
        >
          {failure && <Alert severity="error">{failure.message}</Alert>}
          <TextField label="Название" required {...field('name')} />
          <TextField
            label="Эмодзи"
            placeholder="🏠"
            {...field('emoji')}
            helperText={errors.emoji ?? 'Необязательно, например 🏠'}
          />
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={onClose} disabled={save.isPending}>
            Отмена
          </Button>
          <Button type="submit" variant="contained" disabled={save.isPending}>
            {save.isPending ? 'Сохраняю…' : 'Сохранить'}
          </Button>
        </DialogActions>
      </Box>
    </Dialog>
  );
}

const stripEmpty = (errors: Record<string, string | undefined>) =>
  Object.fromEntries(Object.entries(errors).filter(([, text]) => text));
