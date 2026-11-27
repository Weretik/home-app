import type {
  FetchInfiniteQueryOptions,
  FetchQueryOptions,
  QueryClient,
  QueryKey,
} from '@tanstack/react-query';
import { budgetQuery } from '../../features/expenses/budget-data/budget-api';
import {
  expenseCategoriesQuery,
  expensesQuery,
  incomesQuery,
} from '../../features/expenses/data/expenses-api';
import { currentMonth } from '../../features/expenses/data/month';
import {
  savingsMovementsQuery,
  savingsOverviewQuery,
} from '../../features/expenses/data/savings-api';
import { plannedExpensesQuery } from '../../features/expenses/planned-data/planned-api';
import {
  giftCategoriesQuery,
  giftDetailsQuery,
  giftsQuery,
} from '../../features/gifts/data/gifts-api';
import { monthSummaryQuery } from '../../features/home/expenses-summary/data/summary-api';
import { giftsSummaryQuery } from '../../features/home/gifts-summary/data/summary-api';
import { shoppingSummaryQuery } from '../../features/home/shopping-summary/data/shopping-summary-api';
import { todoSummaryQuery } from '../../features/home/todos-summary/data/todos-summary-api';
import { categoriesQuery as noteCategoriesQuery } from '../../features/notes/data/categories-api';
import { notesQuery } from '../../features/notes/data/notes-api';
import { tabOptions } from '../../features/notes/section/notes-tabs';
import { toNoteQuery } from '../../features/notes/url-state/filters';
import { notificationsStatusQuery } from '../../features/notifications/data/notifications-api';
import { pushStatusQuery } from '../../features/notifications/data/push-api';
import { departmentsQuery } from '../../features/shopping/department-settings/departments-api';
import { shoppingHistoryQuery } from '../../features/shopping/history-page/history-api';
import { keywordsQuery } from '../../features/shopping/keyword-settings/keywords-api';
import { shoppingSettingsQuery } from '../../features/shopping/settings-page/settings-api';
import {
  frequentItemsQuery,
  shoppingListQuery,
} from '../../features/shopping/data/shopping-api';
import { categoriesQuery as todoCategoriesQuery } from '../../features/todos/data/categories-api';
import { kyivToday } from '../../features/todos/data/due-date';
import { tasksQuery } from '../../features/todos/data/todos-api';
import {
  resolveWho,
  toTaskQuery,
} from '../../features/todos/url-state/filters';
import type { FamilyPerson } from '../../platform/client/member-api';

/** Data younger than this is not loaded again. */
const FRESH_MS = 60_000;

/** One query to keep offline: its key (for tests) and how to load it. */
export interface OfflineLoad {
  key: QueryKey;
  load: (queryClient: QueryClient) => Promise<void>;
}

// The queries differ in data type; prefetching needs only their key and function, so they are widened here.
const single = (query: { queryKey: QueryKey }): OfflineLoad => ({
  key: query.queryKey,
  load: (client) =>
    client.prefetchQuery({
      ...(query as unknown as FetchQueryOptions),
      staleTime: FRESH_MS,
    }),
});

const paged = (query: { queryKey: QueryKey }): OfflineLoad => ({
  key: query.queryKey,
  load: (client) =>
    client.prefetchInfiniteQuery({
      ...(query as unknown as FetchInfiniteQueryOptions),
      staleTime: FRESH_MS,
    }),
});

/**
 * What every main page shows when opened without filters, so it works offline without being opened first (web ADR
 * 0022): the lists, the home summaries, the current month of the budget and the settings the dialogs need. The keys
 * are the pages' own (each feature's `*Query`), so a page finds its data in the cache.
 */
export function offlineLoads(
  person: FamilyPerson | undefined,
  now = new Date(),
) {
  const month = currentMonth(now);
  const today = kyivToday(now);
  const tasksFor = (who: ReturnType<typeof resolveWho>) =>
    tasksQuery('active', toTaskQuery({ who }, 'active', today));
  return [
    single(shoppingListQuery()),
    single(frequentItemsQuery()),
    single(shoppingSummaryQuery()),
    single(todoSummaryQuery()),
    single(giftsSummaryQuery()),
    single(monthSummaryQuery(month)),
    single(todoCategoriesQuery()),
    single(giftsQuery(false)),
    single(giftsQuery(true)),
    single(giftCategoriesQuery()),
    single(giftDetailsQuery()),
    single(noteCategoriesQuery()),
    single(budgetQuery()),
    single(expenseCategoriesQuery()),
    single(savingsOverviewQuery()),
    single(plannedExpensesQuery(month)),
    single(notificationsStatusQuery()),
    single(pushStatusQuery()),
    single(departmentsQuery()),
    single(keywordsQuery({})),
    single(shoppingSettingsQuery()),
    ...tabOptions(person).map((tab) =>
      single(notesQuery(toNoteQuery(tab.about, {}))),
    ),
    paged(tasksFor(resolveWho(null, person))),
    paged(tasksFor(undefined)),
    paged(expensesQuery(month, {})),
    paged(incomesQuery(month, {})),
    paged(savingsMovementsQuery(month)),
    paged(shoppingHistoryQuery()),
    paged(
      tasksQuery(
        'archive',
        toTaskQuery({ who: resolveWho(null, person) }, 'archive', today),
      ),
    ),
  ];
}

/** Loads every offline query that is missing or older than a minute; a failure leaves the kept data as it is. */
export function syncOfflineData(
  queryClient: QueryClient,
  person: FamilyPerson | undefined,
) {
  for (const { load } of offlineLoads(person)) void load(queryClient);
}
