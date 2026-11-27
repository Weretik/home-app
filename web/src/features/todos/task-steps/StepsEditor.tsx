import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import List from '@mui/material/List';
import Typography from '@mui/material/Typography';
import { AddStepField } from './AddStepField';
import { StepItem } from './StepItem';
import { MAX_STEPS, type StepsState } from './use-steps';

/** «Кроки»: the checklist of the task with tick, rename in place, delete and «Додати крок» (FR-005). */
export function StepsEditor({ state }: { state: StepsState }) {
  const done = state.steps.filter((step) => step.isDone).length;
  return (
    <Box
      component="section"
      aria-label="Кроки"
      sx={{ display: 'grid', gap: 0.5 }}
    >
      <Typography variant="subtitle1" component="h3" sx={{ fontWeight: 600 }}>
        Кроки
        {state.steps.length > 0 && (
          <Box
            component="span"
            sx={{
              ml: 1,
              color: 'text.secondary',
              fontVariantNumeric: 'tabular-nums',
            }}
          >
            {done}/{state.steps.length}
          </Box>
        )}
      </Typography>
      {state.message && <Alert severity="error">{state.message}</Alert>}
      {state.steps.length > 0 && (
        <List dense disablePadding aria-label="Список кроків">
          {state.steps.map((step) => (
            <StepItem
              key={step.id}
              step={step}
              canTick={!state.isNew}
              onToggle={() => state.toggle(step)}
              onRename={(title) => state.rename(step, title)}
              onRemove={() => state.remove(step)}
            />
          ))}
        </List>
      )}
      {state.isFull ? (
        <Typography variant="body2" color="text.secondary">
          У справі не більше {MAX_STEPS} кроків.
        </Typography>
      ) : (
        <AddStepField disabled={state.isAdding} onAdd={state.add} />
      )}
    </Box>
  );
}
