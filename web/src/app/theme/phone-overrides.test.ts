import { theme } from './theme';

const COARSE = '@media (pointer: coarse)';

type Styles = Record<string, unknown>;
const rootOf = (name: 'MuiIconButton' | 'MuiToggleButton' | 'MuiButton') =>
  theme.components?.[name]?.styleOverrides?.root as Styles;

test('icon and toggle buttons are at least 44px on a touch screen', () => {
  expect(rootOf('MuiIconButton')[COARSE]).toMatchObject({
    minWidth: 44,
    minHeight: 44,
  });
  expect(rootOf('MuiToggleButton')[COARSE]).toMatchObject({ minHeight: 44 });
});

test('buttons keep their look and get a 44px hit area on a touch screen', () => {
  const coarse = rootOf('MuiButton')[COARSE] as Styles;
  expect(coarse['&::after']).toMatchObject({
    width: 'max(100%, 44px)',
    height: 'max(100%, 44px)',
  });
  expect(theme.components?.MuiButton?.defaultProps).toMatchObject({
    disableElevation: true,
  });
});
