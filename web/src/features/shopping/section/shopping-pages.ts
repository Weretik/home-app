/** The pages of the Shopping section, shown as tabs; «Настройки» is reached from the gear, not a tab (design.md). */
export interface ShoppingPage {
  /** Path under /shopping; '' is the index page. */
  path: string;
  label: string;
}

export const shoppingPages: ShoppingPage[] = [
  { path: '', label: 'Список' },
  { path: 'history', label: 'История' },
];

export const sectionPath = '/shopping';

export const pageUrl = (page: ShoppingPage) =>
  page.path ? `${sectionPath}/${page.path}` : sectionPath;

export const settingsPath = `${sectionPath}/settings`;
