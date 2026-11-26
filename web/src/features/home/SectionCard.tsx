import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import type SvgIcon from '@mui/material/SvgIcon';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardActionArea from '@mui/material/CardActionArea';
import Typography from '@mui/material/Typography';
import { Link } from 'react-router-dom';

interface SectionCardProps {
  to: string;
  title: string;
  caption: string;
  icon: typeof SvgIcon;
  color: 'primary' | 'secondary';
}

/** A light card with a tinted icon tile: a link to a section with one line about it. */
export function SectionCard({
  to,
  title,
  caption,
  icon: Icon,
  color,
}: SectionCardProps) {
  return (
    <Card>
      <CardActionArea
        component={Link}
        to={to}
        sx={{ p: 2, display: 'flex', alignItems: 'center', gap: 2 }}
      >
        <Box
          sx={(theme) => ({
            width: 48,
            height: 48,
            flex: 'none',
            borderRadius: 3,
            display: 'grid',
            placeItems: 'center',
            color: `${color}.main`,
            bgcolor: `rgba(${theme.vars.palette[color].mainChannel} / 0.12)`,
          })}
        >
          <Icon />
        </Box>
        <Box sx={{ flex: 1, minWidth: 0 }}>
          <Typography
            variant="subtitle1"
            component="h2"
            sx={{ fontWeight: 600 }}
          >
            {title}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {caption}
          </Typography>
        </Box>
        <ChevronRightIcon sx={{ color: 'text.secondary' }} />
      </CardActionArea>
    </Card>
  );
}
