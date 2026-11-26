import ExpandLessOutlinedIcon from '@mui/icons-material/ExpandLessOutlined';
import ExpandMoreOutlinedIcon from '@mui/icons-material/ExpandMoreOutlined';
import OpenInNewOutlinedIcon from '@mui/icons-material/OpenInNewOutlined';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Collapse from '@mui/material/Collapse';
import Link from '@mui/material/Link';
import Typography from '@mui/material/Typography';
import { useId, useState } from 'react';
import type { GiftVariantItem } from '../data/gifts-api';
import {
  formatPrice,
  formatVariantsCount,
  variantLabel,
} from '../display/formatting';

/** The variants of a wish: a «N варианта» toggle that unfolds their links, each with its price when known. */
export function VariantLinks({ variants }: { variants: GiftVariantItem[] }) {
  const [open, setOpen] = useState(false);
  const listId = useId();
  if (variants.length === 0) return null;

  return (
    <Box>
      <Button
        size="small"
        onClick={() => setOpen((value) => !value)}
        aria-expanded={open}
        aria-controls={listId}
        endIcon={open ? <ExpandLessOutlinedIcon /> : <ExpandMoreOutlinedIcon />}
        sx={{ px: 1, ml: -1 }}
      >
        {formatVariantsCount(variants.length)}
      </Button>
      <Collapse in={open} unmountOnExit>
        <Box
          component="ul"
          id={listId}
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
                gap: 1,
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
                  sx={{
                    whiteSpace: 'nowrap',
                    fontVariantNumeric: 'tabular-nums',
                  }}
                >
                  {formatPrice(variant.price)}
                </Typography>
              )}
            </Box>
          ))}
        </Box>
      </Collapse>
    </Box>
  );
}
