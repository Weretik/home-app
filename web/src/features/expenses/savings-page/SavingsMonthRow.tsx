import Box from '@mui/material/Box';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemText from '@mui/material/ListItemText';
import Typography from '@mui/material/Typography';
import { formatAmount } from '../data/formatting';
import { formatMonth, type Month } from '../data/month';
import type { SavingsMonthSummary } from '../data/savings-api';
import { SignedAmount } from './SignedAmount';

interface SavingsMonthRowProps {
  month: SavingsMonthSummary;
  /** The month the page shows; marked as current. */
  isOpen: boolean;
  onOpen: (month: Month) => void;
}

/** One month of «По місяцях»: its name, put in and taken out, and the balance at its end; a tap opens it. */
export function SavingsMonthRow({
  month,
  isOpen,
  onOpen,
}: SavingsMonthRowProps) {
  return (
    <ListItemButton
      selected={isOpen}
      aria-current={isOpen ? 'true' : undefined}
      onClick={() => onOpen({ year: month.year, month: month.month })}
      sx={{ gap: 2, alignItems: 'flex-start' }}
    >
      <ListItemText
        sx={{ my: 0 }}
        primary={formatMonth(month)}
        secondary={
          <Box
            component="span"
            sx={{ display: 'flex', gap: 1.5, flexWrap: 'wrap' }}
          >
            <SignedAmount
              component="span"
              variant="body2"
              value={month.deposits}
            />
            <SignedAmount
              component="span"
              variant="body2"
              value={-month.withdrawals}
            />
          </Box>
        }
      />
      <Box sx={{ textAlign: 'right' }}>
        <Typography variant="caption" color="text.secondary">
          на кінець
        </Typography>
        <Typography
          sx={{
            fontWeight: 600,
            whiteSpace: 'nowrap',
            fontVariantNumeric: 'tabular-nums',
          }}
        >
          {formatAmount(month.balanceAtEnd)}
        </Typography>
      </Box>
    </ListItemButton>
  );
}
