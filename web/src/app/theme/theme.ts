import type {} from '@mui/material/themeCssVarsAugmentation';
import { createTheme } from '@mui/material/styles';
import { darkScheme, fonts, lightScheme } from './palette';

const heading = { fontFamily: fonts.heading, fontWeight: 700 };

/**
 * One theme with light and dark colour schemes as CSS variables.
 * The mode (light, dark or system) is chosen in ColorModeMenu and kept in localStorage by MUI.
 */
export const theme = createTheme({
  cssVariables: { colorSchemeSelector: 'data-color-scheme' },
  colorSchemes: { light: lightScheme, dark: darkScheme },
  shape: { borderRadius: 12 },
  typography: {
    fontFamily: fonts.body,
    h1: heading,
    h2: heading,
    h3: heading,
    h4: { ...heading, fontSize: '1.75rem' },
    h5: { ...heading, fontSize: '1.375rem' },
    h6: { ...heading, fontSize: '1.125rem' },
    overline: { fontWeight: 700, letterSpacing: '0.08em' },
    button: { fontWeight: 600, textTransform: 'none' },
  },
  components: {
    MuiButton: { defaultProps: { disableElevation: true } },
    MuiTextField: { defaultProps: { fullWidth: true, size: 'small' } },
    MuiCard: {
      defaultProps: { elevation: 0 },
      styleOverrides: {
        root: ({ theme }) => ({
          borderRadius: 16,
          border: `1px solid ${theme.vars.palette.divider}`,
          boxShadow:
            '0 1px 2px rgba(18, 24, 38, 0.04), 0 8px 24px -12px rgba(18, 24, 38, 0.12)',
          ...theme.applyStyles('dark', { boxShadow: 'none' }),
        }),
      },
    },
    MuiCardContent: {
      styleOverrides: {
        root: { padding: 20, '&:last-child': { paddingBottom: 20 } },
      },
    },
  },
});
