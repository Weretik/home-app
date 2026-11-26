import MenuItem from '@mui/material/MenuItem';
import TextField from '@mui/material/TextField';
import { useIdempotencyKey } from '../data/item-writes-api';
import type { Department } from '../department-settings/departments-api';
import { FormDialog } from '../page-parts/FormDialog';
import { useCheckedForm } from '../page-parts/use-checked-form';
import { checkKeyword, keywordRequest, keywordValues } from './keyword-form';
import { useSaveKeyword, type Keyword } from './keywords-api';

interface KeywordDialogProps {
  keyword?: Keyword;
  /** The department the filter shows, preselected for a new keyword. */
  departmentId?: string;
  departments: Department[];
  onClose: () => void;
}

/** «Новое слово» / «Изменить слово»: the start of a word («зубн паст») and its department. */
export function KeywordDialog({
  keyword,
  departmentId,
  departments,
  onClose,
}: KeywordDialogProps) {
  const form = useCheckedForm(
    () => keywordValues(keyword, departmentId),
    checkKeyword,
  );
  const save = useSaveKeyword();
  const idempotency = useIdempotencyKey();
  const { values, setValues, errorOf } = form;

  const submit = () =>
    form.submit((checked, fail) => {
      const body = keywordRequest(checked);
      save.mutate(
        { body, edit: keyword, idempotencyKey: idempotency.keyFor(body) },
        {
          onSuccess: onClose,
          onError: (e) => {
            if (fail(e).renewKey) idempotency.renew();
          },
        },
      );
    });

  return (
    <FormDialog
      title={keyword ? 'Изменить слово' : 'Новое слово'}
      error={form.message}
      isPending={save.isPending}
      onSubmit={submit}
      onClose={onClose}
    >
      <TextField
        label="Слово"
        required
        value={values.keyword}
        onChange={(e) => setValues({ ...values, keyword: e.target.value })}
        error={!!errorOf('keyword')}
        helperText={
          errorOf('keyword') ??
          'Начало слова: «молок» найдёт «молоко» и «молочный»'
        }
      />
      <TextField
        select
        label="Отдел"
        required
        value={values.departmentId}
        onChange={(e) => setValues({ ...values, departmentId: e.target.value })}
        error={!!errorOf('departmentId')}
        helperText={errorOf('departmentId')}
      >
        {departments.map((d) => (
          <MenuItem key={d.id} value={d.id}>
            {d.icon} {d.name}
          </MenuItem>
        ))}
      </TextField>
    </FormDialog>
  );
}
