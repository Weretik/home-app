import Button from '@mui/material/Button';
import { useState } from 'react';
import { useDeleteItem, useUpdateItem } from '../data/item-writes-api';
import { itemTitle } from '../data/labels';
import type { ShoppingItem } from '../data/shopping-api';
import { describeWriteError } from '../data/write-failure-api';
import { useDepartments } from '../department-settings/departments-api';
import { ConfirmDialog } from '../page-parts/ConfirmDialog';
import { FormDialog } from '../page-parts/FormDialog';
import { useCheckedForm } from '../page-parts/use-checked-form';
import { ItemFields } from './ItemFields';
import { checkItem, itemRequest, itemValues } from './item-form';

interface ItemDialogProps {
  item: ShoppingItem;
  onClose: () => void;
}

/** «Изменить товар» (R-6): saves with the item's version in If-Match; «Удалить» asks first. */
export function ItemDialog({ item, onClose }: ItemDialogProps) {
  const form = useCheckedForm(() => itemValues(item), checkItem);
  const [confirming, setConfirming] = useState(false);
  const departments = useDepartments();
  const update = useUpdateItem();
  const remove = useDeleteItem();

  const save = () =>
    form.submit((values, fail) =>
      update.mutate(
        { row: item, body: itemRequest(values) },
        { onSuccess: onClose, onError: fail },
      ),
    );

  if (confirming)
    return (
      <ConfirmDialog
        title="Удалить из списка?"
        summary={itemTitle(item)}
        consequence="Товар уйдёт из списка, не попав в историю."
        error={
          remove.error
            ? describeWriteError(remove.error, 'удалить').message
            : undefined
        }
        isPending={remove.isPending}
        onConfirm={() => remove.mutate(item, { onSuccess: onClose })}
        onClose={() => setConfirming(false)}
      />
    );

  return (
    <FormDialog
      title="Изменить товар"
      error={form.message}
      isPending={update.isPending}
      onSubmit={save}
      onClose={onClose}
      extraAction={
        <Button
          color="error"
          onClick={() => setConfirming(true)}
          disabled={update.isPending}
        >
          Удалить
        </Button>
      }
    >
      <ItemFields
        values={form.values}
        errors={{
          name: form.errorOf('name'),
          quantity: form.errorOf('quantity'),
          note: form.errorOf('note'),
          departmentId: form.errorOf('departmentId'),
        }}
        departments={departments.data ?? []}
        onChange={form.setValues}
      />
    </FormDialog>
  );
}
