import { useSyncExternalStore } from 'react';
import { useCurrentMember } from '../../../platform/member-api';
import type { ShoppingPerson } from '../data/shopping-api';

/**
 * Where a hand-picked «Я» is kept: per signed-in e-mail and only for this tab (sessionStorage), so the next visit
 * starts on the signed-in person again (web ADR 0016, superseding the per-device choice of web ADR 0012).
 */
export const meStorageKey = (email: string) => `home.shopping.me:${email}`;
const fallback: ShoppingPerson = 'Vitaliy';
const people: readonly ShoppingPerson[] = ['Vitaliy', 'Maria'];
const listeners = new Set<() => void>();
/** The choices when storage refuses to keep them. */
const memory = new Map<string, ShoppingPerson>();

/** The person picked by hand in this tab, or null when nobody was picked (or storage holds something else). */
export function readMe(email: string): ShoppingPerson | null {
  const kept = memory.get(email);
  if (kept) return kept;
  try {
    const stored = window.sessionStorage.getItem(meStorageKey(email));
    return people.find((p) => p === stored) ?? null;
  } catch {
    return null;
  }
}

/** Keeps a hand-picked person for this tab and tells every component that shows it. */
export function writeMe(email: string, person: ShoppingPerson) {
  try {
    window.sessionStorage.setItem(meStorageKey(email), person);
  } catch {
    // Private mode or blocked storage: the choice lasts until the page closes.
    memory.set(email, person);
  }
  listeners.forEach((listener) => listener());
}

function subscribe(listener: () => void) {
  listeners.add(listener);
  return () => listeners.delete(listener);
}

/**
 * «Я»: who adds and buys items. The person picked by hand in this tab, else the signed-in person, else Віталій (while
 * the member loads, or for a member who is neither). Shared by the header toggle and the pages.
 */
export function useMe(): [ShoppingPerson, (person: ShoppingPerson) => void] {
  const { data: member } = useCurrentMember();
  const email = member?.email ?? '';
  const picked = useSyncExternalStore(
    subscribe,
    () => readMe(email),
    () => null,
  );
  const me = picked ?? member?.person ?? fallback;
  return [me, (person) => writeMe(email, person)];
}
