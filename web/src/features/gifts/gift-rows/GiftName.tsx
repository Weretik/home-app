import OpenInNewOutlinedIcon from '@mui/icons-material/OpenInNewOutlined';
import Box from '@mui/material/Box';
import IconButton from '@mui/material/IconButton';
import Tooltip from '@mui/material/Tooltip';
import Typography from '@mui/material/Typography';
import type { GiftItem } from '../data/gifts-api';

/** A gift's name with its shop link as an icon, and its notes in at most two lines under it. */
export function GiftName({ gift }: { gift: GiftItem }) {
  return (
    <Box sx={{ minWidth: 0 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
        <Typography sx={{ fontWeight: 600, overflowWrap: 'anywhere' }}>
          {gift.name}
        </Typography>
        {gift.link && (
          <Tooltip title="Открыть ссылку">
            <IconButton
              size="small"
              href={gift.link}
              target="_blank"
              rel="noopener noreferrer"
              aria-label={`Открыть ссылку: ${gift.name}`}
              sx={{ my: -0.5 }}
            >
              <OpenInNewOutlinedIcon sx={{ fontSize: 16 }} />
            </IconButton>
          </Tooltip>
        )}
      </Box>
      {gift.notes && (
        <Typography
          variant="body2"
          color="text.secondary"
          title={gift.notes}
          sx={{
            display: '-webkit-box',
            WebkitLineClamp: 2,
            WebkitBoxOrient: 'vertical',
            overflow: 'hidden',
            overflowWrap: 'anywhere',
          }}
        >
          {gift.notes}
        </Typography>
      )}
    </Box>
  );
}
