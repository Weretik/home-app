import AddOutlinedIcon from '@mui/icons-material/AddOutlined';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import Typography from '@mui/material/Typography';
import { useState } from 'react';
import { describeWriteError } from '../data/write-failure-api';
import { useDepartments } from '../department-settings/departments-api';
import { ConfirmDialog } from '../page-parts/ConfirmDialog';
import { useDebouncedValue } from '../quick-add/name-suggestions/use-debounced-value';
import { KeywordDialog } from './KeywordDialog';
import { KeywordFilters } from './KeywordFilters';
import { KeywordRows } from './KeywordRows';
import { useDeleteKeyword, useKeywords, type Keyword } from './keywords-api';

const STEP = 50;
type Open = { kind: 'add' } | { kind: 'edit' | 'delete'; keyword: Keyword };

/** «Слова для отделов»: search, department filter, add / edit / delete; learned words carry «выучено» (R-12). */
export function KeywordsCard() {
  const [search, setSearch] = useState('');
  const [departmentId, setDepartmentId] = useState('');
  const [shown, setShown] = useState(STEP);
  const [open, setOpen] = useState<Open | null>(null);
  const settled = useDebouncedValue(search.trim());
  const departments = useDepartments().data ?? [];
  const keywords = useKeywords({
    search: settled || undefined,
    departmentId: departmentId || undefined,
  });
  const remove = useDeleteKeyword();
  const rows = keywords.data ?? [];
  const close = () => {
    remove.reset();
    setOpen(null);
  };

  return (
    <Card
      component="section"
      aria-labelledby="keywords-title"
      sx={{ display: 'grid', gap: 1.5, pt: 2 }}
    >
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          gap: 1,
          px: 2,
        }}
      >
        <Typography
          id="keywords-title"
          variant="subtitle1"
          component="h2"
          sx={{ fontWeight: 600 }}
        >
          Слова для отделов
        </Typography>
        <Button
          startIcon={<AddOutlinedIcon />}
          onClick={() => setOpen({ kind: 'add' })}
        >
          Добавить слово
        </Button>
      </Box>
      <KeywordFilters
        search={search}
        departmentId={departmentId}
        departments={departments}
        onSearch={(value) => {
          setSearch(value);
          setShown(STEP);
        }}
        onDepartment={(value) => {
          setDepartmentId(value);
          setShown(STEP);
        }}
      />
      {keywords.error && (
        <Alert severity="error" sx={{ mx: 2 }}>
          Не удалось загрузить слова: {keywords.error.message}
        </Alert>
      )}
      {keywords.isSuccess && rows.length === 0 && (
        <Typography sx={{ color: 'text.secondary', px: 2 }}>
          Слов не найдено. Измените поиск или добавьте слово.
        </Typography>
      )}
      <KeywordRows
        rows={rows.slice(0, shown)}
        onEdit={(keyword) => setOpen({ kind: 'edit', keyword })}
        onDelete={(keyword) => setOpen({ kind: 'delete', keyword })}
      />
      {rows.length > shown && (
        <Button
          onClick={() => setShown(shown + STEP)}
          sx={{ justifySelf: 'center', mb: 1 }}
        >
          Показать ещё ({rows.length - shown})
        </Button>
      )}
      {(open?.kind === 'add' || open?.kind === 'edit') && (
        <KeywordDialog
          keyword={open.kind === 'edit' ? open.keyword : undefined}
          departmentId={departmentId}
          departments={departments}
          onClose={() => setOpen(null)}
        />
      )}
      {open?.kind === 'delete' && (
        <ConfirmDialog
          title="Удалить слово?"
          summary={`${open.keyword.keyword} → ${open.keyword.departmentName}`}
          consequence="Товары с этим словом будут попадать в отдел по другим словам или в «Другое»."
          error={
            remove.error
              ? describeWriteError(remove.error, 'удалить').message
              : undefined
          }
          isPending={remove.isPending}
          onConfirm={() => remove.mutate(open.keyword, { onSuccess: close })}
          onClose={close}
        />
      )}
    </Card>
  );
}
