import BlockOutlinedIcon from '@mui/icons-material/BlockOutlined';
import FavoriteBorderOutlinedIcon from '@mui/icons-material/FavoriteBorderOutlined';
import LightbulbOutlinedIcon from '@mui/icons-material/LightbulbOutlined';
import Box from '@mui/material/Box';
import { kindLabels } from '../data/labels';
import type { NoteKind } from '../data/notes-api';

/** The kind's colour and icon (FR-004): green heart «Любить», red crossed circle «Не любить», blue lightbulb «Факт». */
const marks = {
  Likes: { tone: 'success', Icon: FavoriteBorderOutlinedIcon },
  Dislikes: { tone: 'error', Icon: BlockOutlinedIcon },
  Fact: { tone: 'info', Icon: LightbulbOutlinedIcon },
} as const;

/**
 * The kind on a card: the icon in the kind's colour on a 0.12 tint of it, the word in `text.primary`, so the contrast
 * holds in both schemes and the colour is never the only hint.
 */
export function KindMark({ kind }: { kind: NoteKind }) {
  const { tone, Icon } = marks[kind];
  return (
    <Box
      component="span"
      sx={(theme) => ({
        display: 'inline-flex',
        alignItems: 'center',
        gap: 0.5,
        px: 1,
        py: 0.25,
        borderRadius: 2,
        typography: 'caption',
        fontWeight: 600,
        color: 'text.primary',
        bgcolor: `rgba(${theme.vars.palette[tone].mainChannel} / 0.12)`,
      })}
    >
      <Icon aria-hidden sx={{ fontSize: 16, color: `${tone}.main` }} />
      {kindLabels[kind]}
    </Box>
  );
}
