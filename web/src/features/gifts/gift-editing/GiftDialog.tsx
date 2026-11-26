import type { GiftCategoryItem, GiftItem } from '../data/gifts-api';
import { useSaveGift } from '../data/gift-writes-api';
import { EditDialog } from '../editing/EditDialog';
import { useEditForm } from '../editing/use-edit-form';
import { VariantsEditor } from '../variant-editing/VariantsEditor';
import {
  emptyGift,
  fromGift,
  toGiftRequest,
  validateGift,
  type GiftPreset,
} from './gift-form';
import { GiftFields } from './GiftFields';
import { OccasionFields } from './OccasionFields';

interface GiftDialogProps {
  /** The gift to change; undefined adds a new one. */
  gift?: GiftItem;
  preset?: GiftPreset;
  categories: GiftCategoryItem[];
  onClose: () => void;
}

/** «Новый подарок» / «Изменить подарок»: every field of a gift and its variants. */
export function GiftDialog({
  gift,
  preset,
  categories,
  onClose,
}: GiftDialogProps) {
  const save = useSaveGift();
  const form = useEditForm({
    initial: gift ? fromGift(gift) : emptyGift(preset),
    validate: validateGift,
    toBody: toGiftRequest,
    edit: gift ? { id: gift.id, version: gift.version } : undefined,
    save,
    onSaved: onClose,
  });

  return (
    <EditDialog
      title={gift ? 'Изменить подарок' : 'Новый подарок'}
      error={form.message}
      isPending={form.isPending}
      onSubmit={form.submit}
      onClose={onClose}
    >
      <GiftFields
        values={form.values}
        errors={form.errors}
        categories={categories}
        set={form.set}
      />
      <VariantsEditor
        variants={form.values.variants}
        errors={form.errors}
        onChange={(variants) => form.set('variants', variants)}
      />
      <OccasionFields
        values={form.values}
        errors={form.errors}
        set={form.set}
      />
    </EditDialog>
  );
}
