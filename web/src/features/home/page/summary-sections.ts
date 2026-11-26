import CardGiftcardOutlinedIcon from '@mui/icons-material/CardGiftcardOutlined';
import ChecklistOutlinedIcon from '@mui/icons-material/ChecklistOutlined';
import ReceiptLongOutlinedIcon from '@mui/icons-material/ReceiptLongOutlined';
import ShoppingCartOutlinedIcon from '@mui/icons-material/ShoppingCartOutlined';
import type { ComponentType } from 'react';
import { ExpensesSummary } from '../expenses-summary/overview/ExpensesSummary';

export interface SummarySection {
  /** The value of `?s=` that opens this summary. */
  key: string;
  label: string;
  icon: typeof ReceiptLongOutlinedIcon;
  /** The summary itself; null while the section has none, and the switcher shows it as «скоро». */
  Summary: ComponentType | null;
}

/**
 * The summaries the home page switches between, in the switcher's order. A section's summary is one entry here
 * (its component in its own folder of this feature); the first built one opens by default.
 */
export const summarySections: SummarySection[] = [
  {
    key: 'expenses',
    label: 'Бюджет',
    icon: ReceiptLongOutlinedIcon,
    Summary: ExpensesSummary,
  },
  { key: 'todos', label: 'Дела', icon: ChecklistOutlinedIcon, Summary: null },
  {
    key: 'shopping',
    label: 'Покупки',
    icon: ShoppingCartOutlinedIcon,
    Summary: null,
  },
  {
    key: 'gifts',
    label: 'Подарки',
    icon: CardGiftcardOutlinedIcon,
    Summary: null,
  },
];
