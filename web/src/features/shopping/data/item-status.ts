import type {
  ShoppingItem,
  ShoppingList,
  ShoppingPerson,
} from './shopping-api';

export type ItemStatus = ShoppingItem['status'];

export interface StatusChange {
  id: string;
  status: ItemStatus;
  by: ShoppingPerson;
}

/**
 * The list as it will look after a checkbox, before the API answers (optimistic update): a bought item gets the
 * buyer and now as its time, an item needed again loses them. Other rows and an unknown id are left as they are.
 */
export function withStatus(
  list: ShoppingList,
  change: StatusChange,
  now: Date = new Date(),
): ShoppingList {
  return {
    ...list,
    items: list.items.map((item) => {
      if (item.id !== change.id || item.status === change.status) return item;
      return change.status === 'Bought'
        ? {
            ...item,
            status: change.status,
            boughtBy: change.by,
            boughtAtUtc: now.toISOString(),
          }
        : { ...item, status: change.status, boughtBy: null, boughtAtUtc: null };
    }),
  };
}
