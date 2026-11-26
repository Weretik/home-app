import { describe, expect, it } from 'vitest';
import { theme } from '../../../../app/theme/theme';
import { personColors, personTint } from './person-colors';

describe('person colours', () => {
  it('gives each person their own theme colour', () => {
    expect(personColors).toEqual({
      Vitaliy: 'primary',
      Maria: 'secondary',
      Together: 'info',
    });
  });

  it('tints a block with the person colour through the theme variables', () => {
    expect(personTint(theme, 'Maria')).toMatch(
      /^rgba\(var\(--mui-palette-secondary-mainChannel.*\) \/ 0\.08\)$/,
    );
    expect(personTint(theme, 'Together', 0.12)).toContain('info');
  });
});
