import { describe, expect, it } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { recipientTones, toneColor, toneTint } from './recipient-tones';

describe('recipient tones', () => {
  it('gives each person their own theme colour', () => {
    expect(recipientTones).toEqual({
      Vitaliy: 'primary',
      Maria: 'secondary',
      Together: 'info',
    });
  });

  it('reads the colour and its tint through the theme variables', () => {
    expect(toneColor(theme, 'Vitaliy')).toContain('--mui-palette-primary-main');
    expect(toneTint(theme, 'Maria')).toMatch(
      /^rgba\(var\(--mui-palette-secondary-mainChannel.*\) \/ 0\.08\)$/,
    );
    expect(toneTint(theme, 'Together', 0.12)).toMatch(/info.*\/ 0\.12\)$/);
  });
});
