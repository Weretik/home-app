import MenuItem from '@mui/material/MenuItem';
import TextField from '@mui/material/TextField';
import type { GiftDetailItem, Recipient } from '../data/gifts-api';
import { useSaveDetail } from '../data/settings-writes-api';
import { recipientLabels, recipients } from '../display/labels';
import { EditDialog } from '../editing/EditDialog';
import { useEditForm } from '../editing/use-edit-form';
import {
  emptyDetail,
  fromDetail,
  toDetailRequest,
  validateDetail,
} from './detail-form';

interface DetailDialogProps {
  detail?: GiftDetailItem;
  preset?: Recipient;
  onClose: () => void;
}

/** «Новый нюанс» / «Изменить нюанс»: for whom, what and its value. */
export function DetailDialog({ detail, preset, onClose }: DetailDialogProps) {
  const form = useEditForm({
    initial: detail ? fromDetail(detail) : emptyDetail(preset),
    validate: validateDetail,
    toBody: toDetailRequest,
    edit: detail ? { id: detail.id, version: detail.version } : undefined,
    save: useSaveDetail(),
    onSaved: onClose,
  });
  const { values, errors, set } = form;

  return (
    <EditDialog
      title={detail ? 'Изменить нюанс' : 'Новый нюанс'}
      error={form.message}
      isPending={form.isPending}
      onSubmit={form.submit}
      onClose={onClose}
    >
      <TextField
        select
        label="Для кого"
        value={values.recipient}
        onChange={(event) => set('recipient', event.target.value as Recipient)}
        error={Boolean(errors.recipient)}
        helperText={errors.recipient}
      >
        {recipients.map((recipient) => (
          <MenuItem key={recipient} value={recipient}>
            {recipientLabels[recipient]}
          </MenuItem>
        ))}
      </TextField>
      <TextField
        label="Что"
        placeholder="Кольцо"
        value={values.label}
        onChange={(event) => set('label', event.target.value)}
        error={Boolean(errors.label)}
        helperText={errors.label}
      />
      <TextField
        label="Значение"
        placeholder="18,5–19,0"
        value={values.value}
        onChange={(event) => set('value', event.target.value)}
        error={Boolean(errors.value)}
        helperText={errors.value}
      />
    </EditDialog>
  );
}
