import AddOutlinedIcon from '@mui/icons-material/AddOutlined';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import CircularProgress from '@mui/material/CircularProgress';
import List from '@mui/material/List';
import Typography from '@mui/material/Typography';
import { useState } from 'react';
import { describeWriteError } from '../data/write-failure-api';
import { ConfirmDialog } from '../page-parts/ConfirmDialog';
import { DepartmentDialog } from './DepartmentDialog';
import { DepartmentRow } from './DepartmentRow';
import { moveId } from './department-order';
import {
  useDeleteDepartment,
  useDepartments,
  useReorderDepartments,
  type Department,
} from './departments-api';

type Open =
  { kind: 'add' } | { kind: 'edit' | 'delete'; department: Department };

/** «Отделы»: the store-walk order (up / down), add, rename, delete with a confirmation (R-11). */
export function DepartmentsCard() {
  const departments = useDepartments();
  const reorder = useReorderDepartments();
  const remove = useDeleteDepartment();
  const [open, setOpen] = useState<Open | null>(null);
  const rows = departments.data ?? [];
  const ids = rows.map((d) => d.id);
  const closeDelete = () => {
    remove.reset();
    setOpen(null);
  };

  return (
    <Card component="section" aria-labelledby="departments-title">
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          gap: 1,
          px: 2,
          pt: 2,
        }}
      >
        <Typography
          id="departments-title"
          variant="subtitle1"
          component="h2"
          sx={{ fontWeight: 600 }}
        >
          Отделы
        </Typography>
        <Button
          startIcon={<AddOutlinedIcon />}
          onClick={() => setOpen({ kind: 'add' })}
        >
          Добавить отдел
        </Button>
      </Box>
      <Typography variant="body2" sx={{ color: 'text.secondary', px: 2 }}>
        В этом порядке идут отделы в списке — как вы обходите магазин.
      </Typography>
      {departments.isPending && (
        <CircularProgress aria-label="Загружаю отделы" sx={{ m: 2 }} />
      )}
      {departments.error && (
        <Alert severity="error" sx={{ m: 2 }}>
          Не удалось загрузить отделы: {departments.error.message}
        </Alert>
      )}
      {reorder.error && (
        <Alert severity="error" sx={{ m: 2 }} onClose={() => reorder.reset()}>
          {describeWriteError(reorder.error, 'изменить порядок').message}
        </Alert>
      )}
      <List aria-labelledby="departments-title">
        {rows.map((department, index) => (
          <DepartmentRow
            key={department.id}
            department={department}
            isFirst={index === 0}
            isLast={index === rows.length - 1}
            isMoving={reorder.isPending}
            onMove={(step) => reorder.mutate(moveId(ids, department.id, step))}
            onEdit={() => setOpen({ kind: 'edit', department })}
            onDelete={() => setOpen({ kind: 'delete', department })}
          />
        ))}
      </List>
      {open?.kind === 'add' && (
        <DepartmentDialog onClose={() => setOpen(null)} />
      )}
      {open?.kind === 'edit' && (
        <DepartmentDialog
          department={open.department}
          onClose={() => setOpen(null)}
        />
      )}
      {open?.kind === 'delete' && (
        <ConfirmDialog
          title="Удалить отдел?"
          summary={`${open.department.icon ?? ''} ${open.department.name}`.trim()}
          consequence="Его товары и слова перейдут в отдел по умолчанию."
          error={
            remove.error
              ? describeWriteError(remove.error, 'удалить').message
              : undefined
          }
          isPending={remove.isPending}
          onConfirm={() =>
            remove.mutate(open.department, { onSuccess: closeDelete })
          }
          onClose={closeDelete}
        />
      )}
    </Card>
  );
}
