import AddOutlinedIcon from '@mui/icons-material/AddOutlined';
import EditNoteOutlinedIcon from '@mui/icons-material/EditNoteOutlined';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import IconButton from '@mui/material/IconButton';
import TextField from '@mui/material/TextField';
import ToggleButton from '@mui/material/ToggleButton';
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup';
import Tooltip from '@mui/material/Tooltip';
import { kindLabels, kinds, people, personNames } from '../data/labels';
import type { NoteKind, NotePerson } from '../data/notes-api';
import type { NoteDraft } from '../note-dialog/note-form';
import { useQuickAdd } from './use-quick-add';

const toggleSx = { textTransform: 'none', px: 1.25 } as const;

/**
 * The quick add on top of the list (FR-003): the kind toggle, in «Усі» also the person, and the text; Enter adds the
 * note, the pencil opens the full dialog with the typed text.
 */
export function QuickAdd({
  about,
  me,
  startKind,
  onOpenForm,
}: {
  about: NotePerson | undefined;
  me: NotePerson | undefined;
  startKind: NoteKind;
  onOpenForm: (draft: NoteDraft) => void;
}) {
  const quick = useQuickAdd({ about, me, startKind });

  return (
    <Card
      component="form"
      onSubmit={quick.submit}
      sx={{ p: 2, display: 'grid', gap: 1.5 }}
    >
      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
        {!about && (
          <ToggleButtonGroup
            exclusive
            size="small"
            color="primary"
            aria-label="Про кого"
            value={quick.person}
            onChange={(_, next: NotePerson | null) =>
              next && quick.setPerson(next)
            }
          >
            {people.map((person) => (
              <ToggleButton key={person} value={person} sx={toggleSx}>
                {personNames[person]}
              </ToggleButton>
            ))}
          </ToggleButtonGroup>
        )}
        <ToggleButtonGroup
          exclusive
          size="small"
          color="primary"
          aria-label="Що"
          value={quick.kind}
          onChange={(_, next: NoteKind | null) => next && quick.setKind(next)}
        >
          {kinds.map((kind) => (
            <ToggleButton key={kind} value={kind} sx={toggleSx}>
              {kindLabels[kind]}
            </ToggleButton>
          ))}
        </ToggleButtonGroup>
      </Box>
      <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1 }}>
        <TextField
          inputRef={quick.input}
          size="small"
          fullWidth
          placeholder={quick.prompt}
          value={quick.text}
          onChange={(event) => quick.setText(event.target.value)}
          error={Boolean(quick.check)}
          helperText={quick.check}
          slotProps={{
            htmlInput: { 'aria-label': 'Нова нотатка', enterKeyHint: 'done' },
          }}
        />
        <Tooltip title="Додати">
          <span>
            <IconButton
              type="submit"
              color="primary"
              aria-label="Додати"
              disabled={quick.isPending}
            >
              <AddOutlinedIcon />
            </IconButton>
          </span>
        </Tooltip>
        <Tooltip title="Детальна форма">
          <IconButton
            aria-label="Детальна форма"
            onClick={() => onOpenForm(quick.draft())}
          >
            <EditNoteOutlinedIcon />
          </IconButton>
        </Tooltip>
      </Box>
      {quick.error && <Alert severity="error">{quick.error}</Alert>}
    </Card>
  );
}
