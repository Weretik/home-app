/**
 * The pages of the Gifts section, shown as tabs (web ADR 0006). «Настройки» is reached from the gear next to the
 * tabs, not a tab itself (docs/design.md).
 */
export interface GiftsPageEntry {
  /** Path under /gifts; '' is the index page. */
  path: string;
  label: string;
}

export const sectionPath = '/gifts';

export const giftsPages: GiftsPageEntry[] = [
  { path: '', label: 'Список' },
  { path: 'archive', label: 'Подаренные' },
];

export const pageUrl = (page: GiftsPageEntry) =>
  page.path ? `${sectionPath}/${page.path}` : sectionPath;

export const settingsPath = `${sectionPath}/settings`;

/** The params a tab switch keeps: the list filters belong to both tabs. */
export const keptParams = ['recipient', 'category'] as const;
