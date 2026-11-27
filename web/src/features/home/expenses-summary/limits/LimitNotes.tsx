import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { limitNotes, numberSx, type LimitRow } from './limit-rows';

/** «перенесення …», «→ наст. міс. …», «план …» under a row, only those that are there; wraps between notes. */
export function LimitNotes({ row }: { row: LimitRow }) {
  const notes = limitNotes(row);
  if (notes.length === 0) return null;
  return (
    <Typography
      variant="body2"
      color="text.secondary"
      sx={{ fontWeight: 400, fontVariantNumeric: 'tabular-nums' }}
    >
      {notes.map((note, index) => (
        <Box component="span" key={note.key}>
          {index > 0 && ' · '}
          <Box component="span" sx={numberSx}>
            {note.text}
          </Box>
        </Box>
      ))}
    </Typography>
  );
}
