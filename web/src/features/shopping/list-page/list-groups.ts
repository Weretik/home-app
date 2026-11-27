import { locale } from '../../../platform/locale';
import type { ShoppingItem } from '../data/shopping-api';

/** One department of the list with its needed items in the order they are picked up. */
export interface DepartmentGroup {
  departmentId: string;
  name: string;
  icon: string | null | undefined;
  sortOrder: number;
  items: ShoppingItem[];
}

const time = (iso: string | null | undefined) => (iso ? Date.parse(iso) : 0);

/**
 * Needed items by department in store-walk order; inside a department urgent ones first, then by the time they were
 * added (oldest first, as in the sheet).
 */
export function groupNeeded(items: ShoppingItem[]): DepartmentGroup[] {
  const groups = new Map<string, DepartmentGroup>();
  for (const item of items) {
    if (item.status !== 'Needed') continue;
    let group = groups.get(item.departmentId);
    if (!group) {
      group = {
        departmentId: item.departmentId,
        name: item.departmentName,
        icon: item.departmentIcon,
        sortOrder: item.departmentSortOrder,
        items: [],
      };
      groups.set(item.departmentId, group);
    }
    group.items.push(item);
  }
  const sorted = [...groups.values()].sort(
    (a, b) => a.sortOrder - b.sortOrder || a.name.localeCompare(b.name, locale),
  );
  for (const group of sorted)
    group.items.sort(
      (a, b) =>
        Number(b.urgent) - Number(a.urgent) ||
        time(a.addedAtUtc) - time(b.addedAtUtc),
    );
  return sorted;
}

/** Bought items still in the cart, the latest bought first. */
export const cartItems = (items: ShoppingItem[]) =>
  items
    .filter((item) => item.status === 'Bought')
    .sort((a, b) => time(b.boughtAtUtc) - time(a.boughtAtUtc));

/** The counts in the section header. */
export function countItems(items: ShoppingItem[]) {
  const needed = items.filter((item) => item.status === 'Needed');
  return {
    needed: needed.length,
    urgent: needed.filter((item) => item.urgent).length,
    inCart: items.length - needed.length,
  };
}
