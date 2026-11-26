import type SvgIcon from '@mui/material/SvgIcon';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardActionArea from '@mui/material/CardActionArea';
import Typography from '@mui/material/Typography';
import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';

interface HighlightCardProps {
  to: string;
  title: string;
  value: ReactNode;
  caption: string;
  icon: typeof SvgIcon;
  color: 'primary' | 'secondary';
}

/**
 * A bright widget card for the top row of a dashboard (docs/design.md, "Cards"):
 * a gradient in the primary or secondary colour with two soft circles, a big value and a caption.
 */
export function HighlightCard({
  to,
  title,
  value,
  caption,
  icon: Icon,
  color,
}: HighlightCardProps) {
  return (
    <Card
      sx={(theme) => ({
        position: 'relative',
        overflow: 'hidden',
        border: 0,
        color: `${color}.contrastText`,
        background: `linear-gradient(140deg, ${theme.vars.palette[color].main} 0%, ${theme.vars.palette[color].dark} 100%)`,
        // In the dark scheme the bright main colours are too light under white text: use the deep shade.
        ...theme.applyStyles('dark', {
          color: theme.vars.palette.common.white,
          background: `linear-gradient(140deg, ${theme.vars.palette[color].dark} 0%, rgba(${theme.vars.palette[color].darkChannel} / 0.7) 100%)`,
        }),
        '&::before, &::after': {
          content: '""',
          position: 'absolute',
          borderRadius: '50%',
          background: 'rgba(255 255 255 / 0.1)',
          pointerEvents: 'none',
        },
        '&::before': { width: 210, height: 210, top: -110, right: -70 },
        '&::after': { width: 210, height: 210, top: -40, right: -140 },
      })}
    >
      <CardActionArea
        component={Link}
        to={to}
        sx={{
          p: 2.5,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'flex-start',
          gap: 1.5,
        }}
      >
        <Box
          sx={{
            width: 44,
            height: 44,
            borderRadius: 3,
            display: 'grid',
            placeItems: 'center',
            bgcolor: 'rgba(255 255 255 / 0.16)',
          }}
        >
          <Icon />
        </Box>
        <Box>
          <Typography
            variant="h4"
            component="p"
            sx={{ fontVariantNumeric: 'tabular-nums' }}
          >
            {value}
          </Typography>
          <Typography
            variant="subtitle1"
            component="h2"
            sx={{ fontWeight: 600 }}
          >
            {title}
          </Typography>
          <Typography variant="body2" sx={{ opacity: 0.8 }}>
            {caption}
          </Typography>
        </Box>
      </CardActionArea>
    </Card>
  );
}
