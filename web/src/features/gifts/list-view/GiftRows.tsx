import useMediaQuery from '@mui/material/useMediaQuery';
import { useTheme } from '@mui/material/styles';
import { GiftList } from '../gift-list/GiftList';
import { GiftTable, type GiftRowsProps } from '../gift-table/GiftTable';

/** The gifts as a table from `md`, as a compact list below it (the Expenses pattern). */
export function GiftRows(props: GiftRowsProps) {
  const theme = useTheme();
  const isWide = useMediaQuery(theme.breakpoints.up('md'));
  return isWide ? <GiftTable {...props} /> : <GiftList {...props} />;
}
