import Typography, { type TypographyProps } from '@mui/material/Typography';
import { formatSignedAmount } from '../data/formatting';

/**
 * A signed amount of the jar (docs/design.md, "Signed amounts"): plus in `primary` blue, minus in `error` red, and
 * the sign is always written, so the colour is never the only hint.
 */
export function SignedAmount({
  value,
  sx,
  ...props
}: { value: number } & Omit<TypographyProps, 'children' | 'color'>) {
  const color =
    value > 0 ? 'primary.main' : value < 0 ? 'error.main' : 'text.primary';
  return (
    <Typography
      {...props}
      sx={[
        { color, whiteSpace: 'nowrap', fontVariantNumeric: 'tabular-nums' },
        ...(Array.isArray(sx) ? sx : [sx]),
      ]}
    >
      {formatSignedAmount(value)}
    </Typography>
  );
}
