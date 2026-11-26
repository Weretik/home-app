import TextField from '@mui/material/TextField';
import { useIdempotencyKey } from '../data/item-writes-api';
import { FormDialog } from '../page-parts/FormDialog';
import { useCheckedForm } from '../page-parts/use-checked-form';
import {
  checkDepartment,
  departmentRequest,
  departmentValues,
} from './department-form';
import { useSaveDepartment, type Department } from './departments-api';

interface DepartmentDialogProps {
  /** The department to rename; left out for a new one. */
  department?: Department;
  onClose: () => void;
}

/** «Новый отдел» / «Изменить отдел»: the name and an emoji icon; a new one sends an Idempotency-Key. */
export function DepartmentDialog({
  department,
  onClose,
}: DepartmentDialogProps) {
  const form = useCheckedForm(
    () => departmentValues(department),
    checkDepartment,
  );
  const save = useSaveDepartment();
  const idempotency = useIdempotencyKey();
  const { values, setValues, errorOf } = form;

  const submit = () =>
    form.submit((checked, fail) => {
      const body = departmentRequest(checked);
      save.mutate(
        { body, edit: department, idempotencyKey: idempotency.keyFor(body) },
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
      title={department ? 'Изменить отдел' : 'Новый отдел'}
      error={form.message}
      isPending={save.isPending}
      onSubmit={submit}
      onClose={onClose}
    >
      <TextField
        label="Название"
        required
        value={values.name}
        onChange={(e) => setValues({ ...values, name: e.target.value })}
        error={!!errorOf('name')}
        helperText={errorOf('name')}
      />
      <TextField
        label="Значок"
        placeholder="🥛"
        value={values.icon}
        onChange={(e) => setValues({ ...values, icon: e.target.value })}
        error={!!errorOf('icon')}
        helperText={
          errorOf('icon') ?? 'Эмодзи перед названием, можно оставить пустым'
        }
      />
    </FormDialog>
  );
}
