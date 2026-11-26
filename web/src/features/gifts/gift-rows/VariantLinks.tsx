import OpenInNewOutlinedIcon from '@mui/icons-material/OpenInNewOutlined';
import Box from '@mui/material/Box';
import Link from '@mui/material/Link';
import Typography from '@mui/material/Typography';
import type { GiftVariantItem } from '../data/gifts-api';
import { formatPrice, variantLabel } from '../display/formatting';

/** The links of a gift's variants (title or shop address), each with its price when known; they open in a new tab. */
export function VariantLinks({
  id,
  variants,
}: {
  id: string;
  variants: GiftVariantItem[];
}) {
  return (
    <Box
      component="ul"
      id={id}
      sx={{ m: 0, p: 0, listStyle: 'none', display: 'grid', gap: 0.75 }}
    >
      {variants.map((variant) => (
        <Box
          component="li"
          key={variant.id}
          sx={{
            display: 'flex',
            alignItems: 'baseline',
            justifyContent: 'space-between',
            gap: 2,
            maxWidth: 560,
          }}
        >
          <Link
            href={variant.link}
            target="_blank"
            rel="noopener noreferrer"
            sx={{
              display: 'inline-flex',
              alignItems: 'center',
              gap: 0.5,
              minWidth: 0,
              overflowWrap: 'anywhere',
            }}
          >
            {variantLabel(variant)}
            <OpenInNewOutlinedIcon sx={{ fontSize: 14 }} aria-hidden />
          </Link>
          {variant.price != null && (
            <Typography
              variant="body2"
              sx={{ whiteSpace: 'nowrap', fontVariantNumeric: 'tabular-nums' }}
            >
              {formatPrice(variant.price)}
            </Typography>
          )}
        </Box>
      ))}
    </Box>
  );
}
