import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { groupByCategory } from '../data/note-groups';
import type { NoteItem } from '../data/notes-api';
import { cardGrid } from '../note-card/card-grid';
import { NoteCard } from '../note-card/NoteCard';

interface NoteGroupsProps {
  notes: NoteItem[];
  showPerson: boolean;
  onOpen: (note: NoteItem) => void;
}

/** The notes by category (FR-004): a heading with the emoji and the count, then the cards in a grid. */
export function NoteGroups({ notes, showPerson, onOpen }: NoteGroupsProps) {
  return groupByCategory(notes).map((group) => {
    const headingId = `notes-group-${group.key}`;
    return (
      <Box
        key={group.key}
        component="section"
        aria-labelledby={headingId}
        sx={{ display: 'grid', gap: 1.5, minWidth: 0 }}
      >
        <Typography
          id={headingId}
          variant="subtitle2"
          component="h2"
          sx={{ fontWeight: 600, color: 'text.secondary' }}
        >
          {group.label}{' '}
          <Box component="span" sx={{ fontVariantNumeric: 'tabular-nums' }}>
            · {group.notes.length}
          </Box>
        </Typography>
        <Box
          component="ul"
          aria-labelledby={headingId}
          sx={{ ...cardGrid, m: 0, p: 0 }}
        >
          {group.notes.map((note) => (
            <NoteCard
              key={note.id}
              note={note}
              showPerson={showPerson}
              onOpen={onOpen}
            />
          ))}
        </Box>
      </Box>
    );
  });
}
