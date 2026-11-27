import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import { useState, type FormEvent } from 'react';
import { useCheckedForm } from '../page-parts/use-checked-form';
import {
  checkListSettings,
  DAYS,
  HOURS,
  listSettingsRequest,
  listSettingsValues,
} from './list-settings-form';
import { useSaveShoppingSettings, type ShoppingSettings } from './settings-api';

/** «Список»: hours a bought item stays in the cart and the ⭐ Часті window in days (R-13). */
export function ListSettingsCard({ settings }: { settings: ShoppingSettings }) {
  const form = useCheckedForm(
    () => listSettingsValues(settings),
    checkListSettings,
  );
  const [saved, setSaved] = useState(false);
  const save = useSaveShoppingSettings();
  const { values, setValues } = form;

  const submit = (event: FormEvent) => {
    event.preventDefault();
    setSaved(false);
    form.submit((checked, fail) =>
      save.mutate(
        { version: settings.version, body: listSettingsRequest(checked) },
        { onSuccess: () => setSaved(true), onError: fail },
      ),
    );
  };
  const hoursError = form.errorOf('cartKeepHours');
  const daysError = form.errorOf('frequentDays');

  return (
    <Card
      component="section"
      aria-labelledby="list-settings-title"
      sx={{ p: 2 }}
    >
      <Box
        component="form"
        noValidate
        onSubmit={submit}
        sx={{ display: 'grid', gap: 2 }}
      >
        <Typography
          id="list-settings-title"
          variant="subtitle1"
          component="h2"
          sx={{ fontWeight: 600 }}
        >
          Список
        </Typography>
        {form.message && <Alert severity="error">{form.message}</Alert>}
        <TextField
          label="Скільки годин куплене лежить у кошику"
          value={values.cartKeepHours}
          onChange={(e) =>
            setValues({ ...values, cartKeepHours: e.target.value })
          }
          error={!!hoursError}
          helperText={
            hoursError ??
            `Потім іде в історію. Від ${HOURS.min} до ${HOURS.max}`
          }
          slotProps={{ htmlInput: { inputMode: 'numeric' } }}
        />
        <TextField
          label="За скільки днів шукати часті"
          value={values.frequentDays}
          onChange={(e) =>
            setValues({ ...values, frequentDays: e.target.value })
          }
          error={!!daysError}
          helperText={
            daysError ?? `Для ⭐ Часті. Від ${DAYS.min} до ${DAYS.max}`
          }
          slotProps={{ htmlInput: { inputMode: 'numeric' } }}
        />
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Button type="submit" variant="contained" disabled={save.isPending}>
            {save.isPending ? 'Зберігаю…' : 'Зберегти'}
          </Button>
          {saved && (
            <Typography role="status" sx={{ color: 'text.secondary' }}>
              Збережено
            </Typography>
          )}
        </Box>
      </Box>
    </Card>
  );
}
