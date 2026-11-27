import AccountCircleOutlinedIcon from '@mui/icons-material/AccountCircleOutlined';
import CardGiftcardOutlinedIcon from '@mui/icons-material/CardGiftcardOutlined';
import ChecklistOutlinedIcon from '@mui/icons-material/ChecklistOutlined';
import FavoriteBorderOutlinedIcon from '@mui/icons-material/FavoriteBorderOutlined';
import DashboardOutlinedIcon from '@mui/icons-material/DashboardOutlined';
import MonitorHeartOutlinedIcon from '@mui/icons-material/MonitorHeartOutlined';
import ReceiptLongOutlinedIcon from '@mui/icons-material/ReceiptLongOutlined';
import ShoppingCartOutlinedIcon from '@mui/icons-material/ShoppingCartOutlined';
import TelegramIcon from '@mui/icons-material/Telegram';

export interface Section {
  path: string;
  label: string;
  icon: typeof DashboardOutlinedIcon;
}

/** The main sections: the phone's bottom bar and the top group of the side menu. Keep it at five. */
export const mainSections: Section[] = [
  { path: '/', label: 'Головна', icon: DashboardOutlinedIcon },
  { path: '/expenses', label: 'Бюджет', icon: ReceiptLongOutlinedIcon },
  { path: '/shopping', label: 'Покупки', icon: ShoppingCartOutlinedIcon },
  { path: '/todos', label: 'Справи', icon: ChecklistOutlinedIcon },
  { path: '/gifts', label: 'Подарунки', icon: CardGiftcardOutlinedIcon },
];

/**
 * More sections under the main five: only in the side menu on a computer; on a phone the home page links to them
 * (web ADR 0017).
 */
export const moreSections: Section[] = [
  { path: '/notes', label: 'Про нас', icon: FavoriteBorderOutlinedIcon },
];

/** Service pages: only in the side menu on a computer; on a phone they are linked from the home page. */
export const serviceSections: Section[] = [
  { path: '/status', label: 'Стан', icon: MonitorHeartOutlinedIcon },
  { path: '/notifications', label: 'Телеграм', icon: TelegramIcon },
];

/** The member's own page, reached from the header menu and, on a phone, from «Ще». */
export const profileSection: Section = {
  path: '/profile',
  label: 'Профіль',
  icon: AccountCircleOutlinedIcon,
};

/** What the phone's «Ще» sheet lists: every page that is not in the bottom bar (web ADR 0019). */
export const phoneMoreSections: Section[] = [
  ...moreSections,
  ...serviceSections,
  profileSection,
];

/** The section a path belongs to, for the bottom bar and the page title. */
export function sectionOf(pathname: string): Section | undefined {
  return [...mainSections, ...phoneMoreSections].find((s) =>
    s.path === '/' ? pathname === '/' : pathname.startsWith(s.path),
  );
}
