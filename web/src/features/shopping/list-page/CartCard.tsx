import CleaningServicesOutlinedIcon from '@mui/icons-material/CleaningServicesOutlined';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import List from '@mui/material/List';
import Typography from '@mui/material/Typography';
import { useClearCart } from '../data/item-writes-api';
import type { ShoppingItem } from '../data/shopping-api';
import { describeWriteError } from '../data/write-failure-api';
import { ItemRow } from './ItemRow';

interface CartCardProps {
  items: ShoppingItem[];
  onToggle: (item: ShoppingItem) => void;
  onOpen: (item: ShoppingItem) => void;
}

/** «В корзине (N)»: what was bought lately, struck through; a tap unticks it; «Убрать купленные» clears it (R-7). */
export function CartCard({ items, onToggle, onOpen }: CartCardProps) {
  const clear = useClearCart();
  if (items.length === 0) return null;

  return (
    <Card component="section" aria-labelledby="shopping-cart-title">
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          gap: 1,
          px: 2,
          pt: 1.5,
          flexWrap: 'wrap',
        }}
      >
        <Typography
          id="shopping-cart-title"
          variant="subtitle1"
          component="h2"
          sx={{ fontWeight: 600 }}
        >
          В корзине ({items.length})
        </Typography>
        <Button
          size="small"
          startIcon={<CleaningServicesOutlinedIcon />}
          disabled={clear.isPending}
          onClick={() => clear.mutate()}
        >
          {clear.isPending ? 'Убираю…' : 'Убрать купленные'}
        </Button>
      </Box>
      {clear.error && (
        <Alert severity="error" sx={{ mx: 2, mt: 1 }}>
          {describeWriteError(clear.error, 'убрать купленные').message}
        </Alert>
      )}
      <List aria-labelledby="shopping-cart-title" sx={{ pt: 0.5 }}>
        {items.map((item, index) => (
          <ItemRow
            key={item.id}
            item={item}
            onToggle={onToggle}
            onOpen={onOpen}
            divider={index < items.length - 1}
          />
        ))}
      </List>
    </Card>
  );
}
