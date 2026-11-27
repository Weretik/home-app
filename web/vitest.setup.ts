import '@testing-library/jest-dom/vitest';
import { afterEach } from 'vitest';
import { resetConnectivity } from './src/platform/client/connectivity';

// jsdom has no matchMedia; MUI uses it for the system colour scheme and media queries.
if (typeof window !== 'undefined' && !window.matchMedia) {
  window.matchMedia = (query: string) =>
    ({
      matches: false,
      media: query,
      onchange: null,
      addListener: () => {},
      removeListener: () => {},
      addEventListener: () => {},
      removeEventListener: () => {},
      dispatchEvent: () => false,
    }) as MediaQueryList;
}

// A test that fails a request counts the app offline (src/platform/client/connectivity.ts); the next one starts online.
afterEach(() => resetConnectivity());
