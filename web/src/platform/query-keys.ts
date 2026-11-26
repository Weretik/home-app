/**
 * The query key prefix of each backend module. Every read of a module's API starts its key with the prefix, and every
 * write to the module invalidates the whole prefix, so each page and summary that shows the module's data refreshes,
 * whichever feature shows it (features never import each other; docs/architecture.md, "App").
 */
export const moduleKeys = {
  expenses: ['expenses'],
} as const;
