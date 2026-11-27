import js from '@eslint/js';
import prettier from 'eslint-config-prettier';
import jsxA11y from 'eslint-plugin-jsx-a11y';
import reactHooks from 'eslint-plugin-react-hooks';
import globals from 'globals';
import tseslint from 'typescript-eslint';

export default tseslint.config(
  { ignores: ['dist', 'src/api-client/generated'] },
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      js.configs.recommended,
      ...tseslint.configs.recommended,
      jsxA11y.flatConfigs.recommended,
      prettier,
    ],
    languageOptions: { globals: globals.browser },
    plugins: { 'react-hooks': reactHooks },
    rules: {
      ...reactHooks.configs.recommended.rules,
      // Screens talk to the API only through the typed client (docs/rules.md, "API").
      'no-restricted-globals': [
        'error',
        {
          name: 'fetch',
          message: 'Call the API through src/platform/client/api.ts.',
        },
      ],
      // No other HTTP client, and the openapi-fetch client is made only in src/api-client (docs/rules.md, "API").
      'no-restricted-imports': [
        'error',
        {
          paths: [
            {
              name: 'axios',
              message: 'Call the API through src/platform/client/api.ts.',
            },
            {
              name: 'openapi-fetch',
              message:
                'Only src/api-client builds the client; use src/platform/client/api.ts.',
            },
          ],
        },
      ],
    },
  },
  {
    files: ['src/api-client/**', '**/*.{test,spec}.{ts,tsx}'],
    rules: { 'no-restricted-globals': 'off' },
  },
  {
    files: ['src/api-client/**'],
    rules: { 'no-restricted-imports': 'off' },
  },
  // The service worker runs in its own global scope (web ADR 0019).
  {
    files: ['src/app/service-worker/service-worker.ts'],
    languageOptions: { globals: globals.serviceworker },
  },
);
