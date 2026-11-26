import react from '@vitejs/plugin-react';
import { defineConfig } from 'vitest/config';

export default defineConfig({
  plugins: [react()],
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
