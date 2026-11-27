import OpenInNewOutlinedIcon from '@mui/icons-material/OpenInNewOutlined';
import Box from '@mui/material/Box';
import IconButton from '@mui/material/IconButton';
import Tooltip from '@mui/material/Tooltip';
import Typography from '@mui/material/Typography';
import { useId } from 'react';
import { formatPrice, formatWhole, formatWishes } from '../data/formatting';
import { recipientLabels } from '../data/labels';
import type { GiftsSummaryView } from '../data/summary-api';
import { PersonAvatar } from '../persons/PersonAvatar';
import { personTint } from '../persons/person-colors';

type RecipientSummary = GiftsSummaryView['recipients'][number];

/** One person's 🔥 wishes in their tinted block: avatar, name, count and known sum, then the gifts. */
export function PersonWishes({ group }: { group: RecipientSummary }) {
  const headingId = useId();
  const { recipient, topWanted, counts } = group;
  const knownSum = topWanted.reduce((sum, gift) => sum + (gift.price ?? 0), 0);
  const more = counts.highCount - topWanted.length;

  return (
    <Box
      role="group"
      aria-labelledby={headingId}
      sx={(theme) => ({
        bgcolor: personTint(theme, recipient),
        borderRadius: 3,
        p: { xs: 2, md: 2.5 },
        display: 'grid',
        gap: 1.5,
        alignContent: 'start',
      })}
    >
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
        <PersonAvatar recipient={recipient} size={40} />
        <Box sx={{ minWidth: 0 }}>
          <Typography id={headingId} variant="h6" component="h3">
            {recipientLabels[recipient]}
          </Typography>
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{ fontVariantNumeric: 'tabular-nums' }}
          >
            🔥 {formatWishes(counts.highCount)}
            {knownSum > 0 && ` · ≈ ${formatWhole(knownSum)}`}
          </Typography>
        </Box>
      </Box>
      {topWanted.length === 0 ? (
        <Typography color="text.secondary">Поки немає бажань з 🔥</Typography>
      ) : (
        <Box component="ul" sx={{ m: 0, p: 0, listStyle: 'none' }}>
          {topWanted.map((gift) => (
            <Box
              component="li"
              key={gift.id}
              sx={{
                display: 'flex',
                alignItems: 'center',
                gap: 1.5,
                minHeight: 48,
                py: 0.75,
                '& + &': { borderTop: 1, borderColor: 'divider' },
              }}
            >
              <Typography
                sx={{
                  flex: 1,
                  minWidth: 0,
                  fontSize: '1.0625rem',
                  fontWeight: 500,
                  overflowWrap: 'anywhere',
                }}
              >
                {gift.name}
              </Typography>
              <Typography
                color={gift.price == null ? 'text.secondary' : 'text.primary'}
                variant={gift.price == null ? 'body2' : 'body1'}
                sx={{
                  whiteSpace: 'nowrap',
                  fontWeight: gift.price == null ? 400 : 600,
                  fontVariantNumeric: 'tabular-nums',
                }}
              >
                {formatPrice(gift.price)}
              </Typography>
              {gift.link && (
                <Tooltip title="Відкрити посилання">
                  <IconButton
                    size="small"
                    href={gift.link}
                    target="_blank"
                    rel="noopener noreferrer"
                    aria-label={`Відкрити посилання: ${gift.name}`}
                  >
                    <OpenInNewOutlinedIcon fontSize="small" />
                  </IconButton>
                </Tooltip>
              )}
            </Box>
          ))}
        </Box>
      )}
      {more > 0 && (
        <Typography variant="body2" color="text.secondary">
          І ще {more} у розділі «Подарунки»
        </Typography>
      )}
    </Box>
  );
}
