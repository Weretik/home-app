import CheckCircleOutlinedIcon from '@mui/icons-material/CheckCircleOutlined';
import RadioButtonUncheckedOutlinedIcon from '@mui/icons-material/RadioButtonUncheckedOutlined';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Checkbox from '@mui/material/Checkbox';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import Typography from '@mui/material/Typography';
import {
  assigneeLabels,
  formatDue,
  priorityLabels,
  priorityMarks,
} from '../data/labels';
import {
  describeTickError,
  useCompleteFocusTask,
  type FocusTask,
} from '../data/todos-summary-api';

/** «В фокусе»: up to 8 tasks to look at first; each ticks done in place (FR-008). */
export function FocusList({
  tasks,
  today,
}: {
  tasks: FocusTask[];
  today: string;
}) {
  const complete = useCompleteFocusTask();
  const saving = (id: string) =>
    complete.isPending && complete.variables?.id === id;

  return (
    <>
      {complete.error && (
        <Alert severity="error" sx={{ mx: 2, mb: 1 }}>
          {describeTickError(complete.error)}
        </Alert>
      )}
      <List disablePadding aria-label="В фокусе">
        {tasks.map((task, index) => {
          const overdue = task.dueDate !== null && task.dueDate < today;
          const mark = priorityMarks[task.priority];
          return (
            <ListItem
              key={task.id}
              divider={index < tasks.length - 1}
              sx={{ alignItems: 'flex-start', gap: 0.5, pl: 1 }}
            >
              <Checkbox
                checked={saving(task.id)}
                disabled={complete.isPending}
                onChange={() => complete.mutate(task)}
                icon={<RadioButtonUncheckedOutlinedIcon />}
                checkedIcon={<CheckCircleOutlinedIcon />}
                slotProps={{ input: { 'aria-label': `Готово: ${task.title}` } }}
                sx={{ mt: -0.5 }}
              />
              <Box sx={{ minWidth: 0, py: 0.5 }}>
                <Typography
                  sx={{
                    overflowWrap: 'anywhere',
                    textDecoration: saving(task.id)
                      ? 'line-through'
                      : undefined,
                    color: saving(task.id) ? 'text.secondary' : 'text.primary',
                  }}
                >
                  {task.title}
                </Typography>
                <Typography
                  variant="body2"
                  color="text.secondary"
                  sx={{ display: 'flex', flexWrap: 'wrap', columnGap: 1.25 }}
                >
                  {mark && (
                    <span role="img" aria-label={priorityLabels[task.priority]}>
                      {mark}
                    </span>
                  )}
                  {task.dueDate && (
                    <Box
                      component="span"
                      sx={{
                        color: overdue ? 'error.main' : undefined,
                        fontWeight: overdue ? 600 : undefined,
                      }}
                    >
                      {formatDue(task.dueDate, today)}
                    </Box>
                  )}
                  <span>{assigneeLabels[task.assignee]}</span>
                </Typography>
              </Box>
            </ListItem>
          );
        })}
      </List>
    </>
  );
}
