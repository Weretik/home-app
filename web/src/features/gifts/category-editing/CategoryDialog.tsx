import TextField from '@mui/material/TextField';
import type { GiftCategoryItem } from '../data/gifts-api';
import { useSaveCategory } from '../data/settings-writes-api';
import { EditDialog } from '../editing/EditDialog';
import { useEditForm } from '../editing/use-edit-form';
import { toCategoryRequest, validateCategory } from './category-form';

interface CategoryDialogProps {
  category?: GiftCategoryItem;
  onClose: () => void;
}

/** «Нова категорія» / «Перейменувати категорію»: the name only. */
export function CategoryDialog({ category, onClose }: CategoryDialogProps) {
  const form = useEditForm({
    initial: { name: category?.name ?? '' },
    validate: validateCategory,
    toBody: toCategoryRequest,
    // Categories carry no version; the writer sends no If-Match.
    edit: category ? { id: category.id } : undefined,
    save: useSaveCategory(),
    onSaved: onClose,
  });

  return (
    <EditDialog
      title={category ? 'Перейменувати категорію' : 'Нова категорія'}
      error={form.message}
      isPending={form.isPending}
      onSubmit={form.submit}
      onClose={onClose}
    >
      <TextField
        label="Назва"
        value={form.values.name}
        onChange={(event) => form.set('name', event.target.value)}
        error={Boolean(form.errors.name)}
        helperText={form.errors.name}
      />
    </EditDialog>
  );
}
