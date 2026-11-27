import Typography from '@mui/material/Typography';
import { useDepartmentMatch } from '../../data/shopping-api';
import { useDebouncedValue } from '../name-suggestions/use-debounced-value';

/** Under the field: the department a single typed item will go to («→ 🥛 Молочка»). Several items show nothing. */
export function DepartmentPreview({ text }: { text: string }) {
  const settled = useDebouncedValue(text.trim());
  const single = settled.length > 1 && !/[,;\n]/.test(settled);
  const { data } = useDepartmentMatch(single ? settled : '');

  if (!single || !data || settled !== text.trim()) return null;
  return (
    <Typography
      variant="body2"
      aria-live="polite"
      sx={{ color: 'text.secondary', px: 0.5 }}
    >
      Відділ: {data.departmentIcon} {data.departmentName}
    </Typography>
  );
}
