import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import type { GiftItem } from '../data/gifts-api';
import { toneColor, toneTint } from '../display/recipient-tones';
import type { GiftHandlers } from '../gift-rows/GiftActions';
import { GroupTitle } from '../gift-rows/GroupTitle';
import { groupByRecipient } from '../url-state/gift-filters';
import { GiftTableRow } from './GiftTableRow';

export interface GiftRowsProps extends GiftHandlers {
  items: GiftItem[];
  variant: 'active' | 'archive';
  /** The list's accessible name. */
  label: string;
  /** Per person under a coloured header; false while one person is picked, since the filter already says who. */
  grouped: boolean;
  isChanging: (gift: GiftItem) => boolean;
}

/** Gifts as a table, for a computer (the Expenses pattern): «Список» and «Подаренные» differ in two columns. */
export function GiftTable({
  items,
  variant,
  label,
  grouped,
  isChanging,
  ...handlers
}: GiftRowsProps) {
  const isActive = variant === 'active';
  const headers = [
    'Подарок',
    'Варианты',
    'Категория',
    ...(isActive ? ['Приоритет'] : []),
    'Цена',
    isActive ? 'Повод' : 'Итог',
  ];
  const columns = headers.length + 1;
  const rows = (gifts: GiftItem[]) =>
    gifts.map((gift) => (
      <GiftTableRow
        key={gift.id}
        gift={gift}
        variant={variant}
        isPending={isChanging(gift)}
        columns={columns}
        {...handlers}
      />
    ));

  return (
    <TableContainer>
      <Table size="small" aria-label={label}>
        <TableHead>
          <TableRow>
            {headers.map((header) => (
              <TableCell
                key={header}
                align={header === 'Цена' ? 'right' : undefined}
              >
                {header}
              </TableCell>
            ))}
            <TableCell padding="checkbox" aria-label="Действия" />
          </TableRow>
        </TableHead>
        {!grouped && <TableBody>{rows(items)}</TableBody>}
        {grouped &&
          groupByRecipient(items).map((group) => (
            <TableBody key={group.recipient}>
              <TableRow>
                <TableCell
                  component="th"
                  scope="rowgroup"
                  colSpan={columns}
                  sx={(theme) => ({
                    py: 1.25,
                    bgcolor: toneTint(theme, group.recipient),
                    boxShadow: `inset 4px 0 0 ${toneColor(theme, group.recipient)}`,
                  })}
                >
                  <GroupTitle
                    recipient={group.recipient}
                    count={group.items.length}
                    priceSum={group.priceSum}
                  />
                </TableCell>
              </TableRow>
              {rows(group.items)}
            </TableBody>
          ))}
      </Table>
    </TableContainer>
  );
}
