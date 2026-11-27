import StorefrontOutlinedIcon from '@mui/icons-material/StorefrontOutlined';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardActionArea from '@mui/material/CardActionArea';
import Chip from '@mui/material/Chip';
import Typography from '@mui/material/Typography';
import { noteFooter } from '../data/labels';
import type { NoteItem } from '../data/notes-api';
import { KindMark } from './KindMark';
import { PersonAvatar } from './PersonAvatar';

/**
 * One note (FR-004): the kind mark, the person in «Усі», a star when important, the title, the place, the description
 * clamped to three lines and «Маша · 27.11». A tap opens the dialog. The whole card is one button, so everything in it
 * is a `span` (a button holds phrasing content only; the group's `h2` stays the heading).
 */
export function NoteCard({
  note,
  showPerson,
  onOpen,
}: {
  note: NoteItem;
  showPerson: boolean;
  onOpen: (note: NoteItem) => void;
}) {
  return (
    <Card component="li" sx={{ listStyle: 'none', minWidth: 0 }}>
      <CardActionArea
        onClick={() => onOpen(note)}
        sx={{
          p: 2,
          height: '100%',
          display: 'grid',
          gap: 1,
          alignContent: 'start',
        }}
      >
        <Box
          component="span"
          sx={{ display: 'flex', alignItems: 'center', gap: 1 }}
        >
          <KindMark kind={note.kind} />
          <Box component="span" sx={{ flex: 1 }} />
          {note.isImportant && (
            <Box component="span" role="img" aria-label="Важливо">
              ⭐
            </Box>
          )}
          {showPerson && <PersonAvatar person={note.about} />}
        </Box>
        <Typography
          variant="subtitle1"
          component="span"
          sx={{ display: 'block', fontWeight: 600, overflowWrap: 'anywhere' }}
        >
          {note.title}
        </Typography>
        {note.place && (
          <Chip
            component="span"
            size="small"
            variant="outlined"
            icon={<StorefrontOutlinedIcon />}
            label={note.place}
            sx={{ justifySelf: 'start', maxWidth: '100%' }}
          />
        )}
        {note.details && (
          <Typography
            variant="body2"
            component="span"
            color="text.secondary"
            sx={{
              display: '-webkit-box',
              WebkitLineClamp: 3,
              WebkitBoxOrient: 'vertical',
              overflow: 'hidden',
              overflowWrap: 'anywhere',
              whiteSpace: 'pre-line',
            }}
          >
            {note.details}
          </Typography>
        )}
        <Typography variant="caption" component="span" color="text.secondary">
          {noteFooter(note)}
        </Typography>
      </CardActionArea>
    </Card>
  );
}
