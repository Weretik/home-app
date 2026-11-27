import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import Typography from '@mui/material/Typography';
import { assigneeEmoji, assigneeLabels } from '../data/labels';
import type { AssigneeCounts } from '../data/todos-summary-api';
import { locale } from '../../../../platform/locale';

/** «завдання», «завдання», «завдань» after a count. */
const tasksWord = (count: number) => {
  const forms = { one: 'завдання', few: 'завдання', many: 'завдань' } as const;
  const rule = new Intl.PluralRules(locale).select(count);
  return forms[rule as keyof typeof forms] ?? forms.many;
};

/** Маша, Віталій, Спільне: active tasks, «прострочено N» in red and «сьогодні N» when not zero (FR-008). */
export function AssigneeCards({ counts }: { counts: AssigneeCounts[] }) {
  return (
    <Box
      sx={{
        display: 'grid',
        gap: { xs: 2, md: 3 },
        gridTemplateColumns: { xs: '1fr', sm: 'repeat(3, 1fr)' },
      }}
    >
      {counts.map((count) => {
        const label = assigneeLabels[count.assignee];
        return (
          <Card
            key={count.assignee}
            component="section"
            aria-label={label}
            sx={{ p: 2.5 }}
          >
            <Typography
              variant="subtitle1"
              component="h2"
              sx={{ fontWeight: 600, color: 'text.secondary' }}
            >
              <Box component="span" aria-hidden sx={{ mr: 1 }}>
                {assigneeEmoji[count.assignee]}
              </Box>
              {label}
            </Typography>
            <Typography
              variant="h4"
              component="p"
              sx={{ my: 0.5, fontVariantNumeric: 'tabular-nums' }}
            >
              {count.active}{' '}
              <Typography
                component="span"
                variant="body1"
                color="text.secondary"
                sx={{ ml: 0.5 }}
              >
                {tasksWord(count.active)}
              </Typography>
            </Typography>
            <Typography
              variant="body2"
              color="text.secondary"
              sx={{ fontVariantNumeric: 'tabular-nums' }}
            >
              {count.overdue > 0 && (
                <Box
                  component="span"
                  sx={{ color: 'error.main', fontWeight: 600, mr: 1.5 }}
                >
                  прострочено {count.overdue}
                </Box>
              )}
              {count.dueToday > 0 && <span>сьогодні {count.dueToday}</span>}
              {count.overdue === 0 &&
                count.dueToday === 0 &&
                'на сьогодні все спокійно'}
            </Typography>
          </Card>
        );
      })}
    </Box>
  );
}
