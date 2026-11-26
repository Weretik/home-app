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
        { name: 'fetch', message: 'Call the API through src/platform/api.ts.' },
      ],
    },
  },
  {
    files: ['src/api-client/**', '**/*.{test,spec}.{ts,tsx}'],
    rules: { 'no-restricted-globals': 'off' },
  },
);
