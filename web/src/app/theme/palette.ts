import type { ColorSystemOptions } from '@mui/material/styles';

/**
 * «Синий плащ»: royal blue, steel and silver, with crimson as the second colour.
 * Inspired by the mood of a game character, colours only (ADR 0005).
 */
export const lightScheme: ColorSystemOptions = {
  palette: {
    primary: {
      main: '#1f45b8',
      light: '#4b6fd6',
      dark: '#15338a',
      contrastText: '#ffffff',
    },
    secondary: {
      main: '#b3122e',
      light: '#d13a52',
      dark: '#860c21',
      contrastText: '#ffffff',
    },
    background: { default: '#eef2f6', paper: '#ffffff' },
    text: { primary: '#121826', secondary: '#586174' },
    divider: '#dfe3ec',
  },
};

export const darkScheme: ColorSystemOptions = {
  palette: {
    primary: {
      main: '#5b82ff',
      light: '#8ea8ff',
      dark: '#2f50c9',
      contrastText: '#08122e',
    },
    secondary: {
      main: '#ff5468',
      light: '#ff8090',
      dark: '#c4283d',
      contrastText: '#2b0309',
    },
    background: { default: '#0b0e15', paper: '#151a25' },
    text: { primary: '#e8ecf5', secondary: '#97a1b5' },
    divider: '#242b3a',
  },
};

/** Fonts are self-hosted through @fontsource, imported in main.tsx. */
export const fonts = {
  heading: '"Exo 2", "Segoe UI", system-ui, sans-serif',
  body: '"Inter", "Segoe UI", system-ui, sans-serif',
};
