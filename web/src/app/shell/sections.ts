import CardGiftcardOutlinedIcon from '@mui/icons-material/CardGiftcardOutlined';
import ChecklistOutlinedIcon from '@mui/icons-material/ChecklistOutlined';
import DashboardOutlinedIcon from '@mui/icons-material/DashboardOutlined';
import MonitorHeartOutlinedIcon from '@mui/icons-material/MonitorHeartOutlined';
import ReceiptLongOutlinedIcon from '@mui/icons-material/ReceiptLongOutlined';
import ShoppingCartOutlinedIcon from '@mui/icons-material/ShoppingCartOutlined';

export interface Section {
  path: string;
  label: string;
  icon: typeof DashboardOutlinedIcon;
}

/** The main sections: the phone's bottom bar and the top group of the side menu. Keep it at five. */
export const mainSections: Section[] = [
  { path: '/', label: 'Главная', icon: DashboardOutlinedIcon },
  { path: '/expenses', label: 'Бюджет', icon: ReceiptLongOutlinedIcon },
  { path: '/shopping', label: 'Покупки', icon: ShoppingCartOutlinedIcon },
  { path: '/todos', label: 'Дела', icon: ChecklistOutlinedIcon },
  { path: '/gifts', label: 'Подарки', icon: CardGiftcardOutlinedIcon },
];

/** Service pages: only in the side menu on a computer; on a phone they are linked from the home page. */
export const serviceSections: Section[] = [
  { path: '/status', label: 'Состояние', icon: MonitorHeartOutlinedIcon },
];

/** The section a path belongs to, for the bottom bar and the page title. */
export function sectionOf(pathname: string): Section | undefined {
  return [...mainSections, ...serviceSections].find((s) =>
    s.path === '/' ? pathname === '/' : pathname.startsWith(s.path),
  );
}
