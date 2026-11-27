import type { Components, CssVarsTheme, Theme } from '@mui/material/styles';

/**
 * How controls behave on a phone (web ADR 0020, docs/design.md «Phone and touch»).
 *
 * Bigger tap targets on a touch screen. Only under `(pointer: coarse)`, so a
 * computer with a mouse keeps the compact look. Controls that may grow do (buttons, icon buttons, toggle buttons:
 * at least 44px); small controls inside dense rows keep their look and get an invisible 44px hit area instead.
 */
const COARSE = '@media (pointer: coarse)';
const TARGET = 44;

/** An invisible area around the control that takes the taps, centred on it; the control's look does not change. */
const hitArea = {
  position: 'relative',
  '&::after': {
    content: '""',
    position: 'absolute',
    left: '50%',
    top: '50%',
    width: `max(100%, ${TARGET}px)`,
    height: `max(100%, ${TARGET}px)`,
    transform: 'translate(-50%, -50%)',
  },
} as const;

export const phoneOverrides: Components<
  Omit<Theme, 'components'> & CssVarsTheme
> = {
  MuiButton: {
    styleOverrides: {
      root: {
        [COARSE]: { minHeight: 40, ...hitArea },
      },
      sizeLarge: { [COARSE]: { minHeight: TARGET } },
    },
  },
  MuiIconButton: {
    styleOverrides: {
      root: {
        [COARSE]: {
          minWidth: TARGET,
          minHeight: TARGET,
          '.MuiInputAdornment-root &, .MuiChip-root &': {
            minWidth: 0,
            minHeight: 0,
            ...hitArea,
          },
        },
      },
    },
  },
  MuiToggleButton: {
    styleOverrides: { root: { [COARSE]: { minHeight: TARGET } } },
  },
  MuiChip: {
    styleOverrides: { clickable: { [COARSE]: hitArea } },
  },
  // Section tabs fit more on a narrow screen: no 90px minimum, and a disabled scroll arrow takes no room.
  MuiTab: {
    styleOverrides: {
      root: ({ theme }) => ({
        [theme.breakpoints.down('sm')]: { minWidth: 0, paddingInline: 12 },
      }),
    },
  },
  MuiTabs: {
    styleOverrides: {
      scrollButtons: ({ theme }) => ({
        [theme.breakpoints.down('sm')]: {
          width: 32,
          '&.Mui-disabled': { width: 0 },
        },
      }),
    },
  },
  MuiListItemButton: {
    styleOverrides: { root: { [COARSE]: { minHeight: 48 } } },
  },
};
