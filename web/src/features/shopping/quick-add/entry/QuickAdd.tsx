import AddOutlinedIcon from '@mui/icons-material/AddOutlined';
import PriorityHighOutlinedIcon from '@mui/icons-material/PriorityHighOutlined';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import IconButton from '@mui/material/IconButton';
import Tooltip from '@mui/material/Tooltip';
import { useState, type FormEvent } from 'react';
import { NameField } from '../name-suggestions/NameField';
import { DepartmentPreview } from './DepartmentPreview';
import { FrequentChips } from './FrequentChips';
import type { QuickAddController } from './use-quick-add';

/**
 * The sticky field on top of the list (R-1): several items at once («молоко, хлеб, яйца 10»), Enter adds, the «!»
 * flag marks them urgent; names ever added are suggested while typing (SDD 002); the matched department of a single
 * item and the ⭐ chips are under it.
 */
export function QuickAdd({ quickAdd }: { quickAdd: QuickAddController }) {
  const [text, setText] = useState('');
  const [urgent, setUrgent] = useState(false);

  const submit = (event: FormEvent) => {
    event.preventDefault();
    if (!text.trim() || quickAdd.isPending) return;
    quickAdd.submit(text.trim(), { urgent }, () => {
      setText('');
      setUrgent(false);
    });
  };

  return (
    <Card
      component="section"
      aria-label="Додати до списку"
      sx={{
        position: 'sticky',
        top: 'calc(var(--top-bar-height, 64px) + 8px)',
        zIndex: 2,
        p: { xs: 1.5, md: 2 },
        display: 'grid',
        gap: 1,
      }}
    >
      <Box component="form" onSubmit={submit} sx={{ display: 'flex', gap: 1 }}>
        <NameField
          text={text}
          onTextChange={(next) => {
            setText(next);
            quickAdd.clearError();
          }}
        />
        <Tooltip title={urgent ? 'Терміново: так' : 'Терміново'}>
          <IconButton
            aria-label="Терміново"
            aria-pressed={urgent}
            color={urgent ? 'secondary' : 'default'}
            onClick={() => setUrgent(!urgent)}
          >
            <PriorityHighOutlinedIcon />
          </IconButton>
        </Tooltip>
        <Tooltip title="Додати">
          <span>
            <IconButton
              type="submit"
              aria-label="Додати"
              color="primary"
              disabled={!text.trim() || quickAdd.isPending}
            >
              <AddOutlinedIcon />
            </IconButton>
          </span>
        </Tooltip>
      </Box>
      <DepartmentPreview text={text} />
      {quickAdd.error && <Alert severity="error">{quickAdd.error}</Alert>}
      <FrequentChips quickAdd={quickAdd} />
    </Card>
  );
}
