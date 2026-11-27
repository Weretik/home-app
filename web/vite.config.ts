import { createHash } from 'node:crypto';
import { readFileSync } from 'node:fs';
import react from '@vitejs/plugin-react';
import { VitePWA } from 'vite-plugin-pwa';
import { defineConfig } from 'vitest/config';

// The offline cache's version (web ADR 0022): a new API contract drops the data kept in IndexedDB.
const contractHash = createHash('sha256')
  .update(
    readFileSync(new URL('./src/api-client/generated/api.ts', import.meta.url)),
  )
  .digest('hex')
  .slice(0, 16);

export default defineConfig({
  define: { __API_CONTRACT_HASH__: JSON.stringify(contractHash) },
  plugins: [
    react(),
    // The service worker (web ADR 0021): our own file with the precache manifest injected, registered from the
    // update prompt, never reloading on its own; public/site.webmanifest stays the manifest.
    VitePWA({
      strategies: 'injectManifest',
      srcDir: 'src/app/service-worker',
      filename: 'service-worker.ts',
      registerType: 'prompt',
      injectRegister: false,
      manifest: false,
      injectManifest: {
        globPatterns: ['**/*.{js,css,html,woff2,png,svg,ico,webmanifest}'],
        maximumFileSizeToCacheInBytes: 4 * 1024 * 1024,
      },
      devOptions: { enabled: false },
    }),
  ],
  server: { port: 5173, host: 'localhost' },
  build: { outDir: 'dist', emptyOutDir: true },
  test: {
    globals: true,
    environment: 'jsdom',
    include: ['src/**/*.{test,spec}.{ts,tsx}', 'tools/**/*.test.mjs'],
    setupFiles: ['./vitest.setup.ts'],
    env: { VITE_API_URL: 'http://api.test' },
  },
});
