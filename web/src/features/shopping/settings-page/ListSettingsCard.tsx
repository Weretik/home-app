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

/** «Список»: hours a bought item stays in the cart and the ⭐ Частые window in days (R-13). */
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
          label="Сколько часов купленное лежит в корзине"
          value={values.cartKeepHours}
          onChange={(e) =>
            setValues({ ...values, cartKeepHours: e.target.value })
          }
          error={!!hoursError}
          helperText={
            hoursError ??
            `Потом уходит в историю. От ${HOURS.min} до ${HOURS.max}`
          }
          slotProps={{ htmlInput: { inputMode: 'numeric' } }}
        />
        <TextField
          label="За сколько дней искать частые"
          value={values.frequentDays}
          onChange={(e) =>
            setValues({ ...values, frequentDays: e.target.value })
          }
          error={!!daysError}
          helperText={
            daysError ?? `Для ⭐ Частые. От ${DAYS.min} до ${DAYS.max}`
          }
          slotProps={{ htmlInput: { inputMode: 'numeric' } }}
        />
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Button type="submit" variant="contained" disabled={save.isPending}>
            {save.isPending ? 'Сохраняю…' : 'Сохранить'}
          </Button>
          {saved && (
            <Typography role="status" sx={{ color: 'text.secondary' }}>
              Сохранено
            </Typography>
          )}
        </Box>
      </Box>
    </Card>
  );
}
