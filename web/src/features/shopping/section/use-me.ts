import { useSyncExternalStore } from 'react';
import type { ShoppingPerson } from '../data/shopping-api';

/** Where this device remembers who uses it (web ADR 0012); no sign-in yet. */
export const meStorageKey = 'home.shopping.me';
const fallback: ShoppingPerson = 'Vitaliy';
const people: readonly ShoppingPerson[] = ['Vitaliy', 'Maria'];
const listeners = new Set<() => void>();
/** The choice when storage refuses to keep it. */
let memory: ShoppingPerson | null = null;

/** The stored person; the default when storage is empty, blocked or holds something else. */
export function readMe(): ShoppingPerson {
  try {
    const stored = window.localStorage.getItem(meStorageKey);
    return people.find((p) => p === stored) ?? fallback;
  } catch {
    return fallback;
  }
}

/** Remembers the person on this device and tells every component that shows it. */
export function writeMe(person: ShoppingPerson) {
  try {
    window.localStorage.setItem(meStorageKey, person);
  } catch {
    // Private mode or blocked storage: the choice lasts until the page closes.
    memory = person;
  }
  listeners.forEach((listener) => listener());
}

function subscribe(listener: () => void) {
  listeners.add(listener);
  const onStorage = (event: StorageEvent) => {
    if (event.key === meStorageKey) listener();
  };
  window.addEventListener('storage', onStorage);
  return () => {
    listeners.delete(listener);
    window.removeEventListener('storage', onStorage);
  };
}

const snapshot = () => memory ?? readMe();

/** «Я» on this device: who adds and buys items. Shared by the header toggle and the pages. */
export function useMe(): [ShoppingPerson, (person: ShoppingPerson) => void] {
  const me = useSyncExternalStore(subscribe, snapshot, () => fallback);
  return [me, writeMe];
}
