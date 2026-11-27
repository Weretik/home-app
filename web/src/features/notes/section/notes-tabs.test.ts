import { describe, expect, it } from 'vitest';
import { resolveTab, settingsPath, tabOptions } from './notes-tabs';

describe('tabOptions', () => {
  it('names the tabs relative to the signed-in person (FR-001)', () => {
    expect(tabOptions('Vitaliy')).toEqual([
      { value: 'me', label: 'Про мене', about: 'Vitaliy' },
      { value: 'partner', label: 'Про Машу', about: 'Maria' },
      { value: 'all', label: 'Усі' },
    ]);
    expect(tabOptions('Maria')[1]).toEqual({
      value: 'partner',
      label: 'Про Віталія',
      about: 'Vitaliy',
    });
  });

  it('names the people for a member without a person', () => {
    expect(tabOptions(undefined).map((tab) => tab.label)).toEqual([
      'Віталій',
      'Маша',
      'Усі',
    ]);
  });
});

describe('resolveTab', () => {
  it('starts on «Про мене», or on «Усі» without a person', () => {
    expect(resolveTab(null, 'Maria').value).toBe('me');
    expect(resolveTab(null, undefined).value).toBe('all');
    expect(resolveTab('junk', 'Maria').value).toBe('me');
  });

  it('keeps the tab of the link', () => {
    expect(resolveTab('partner', 'Maria').about).toBe('Vitaliy');
    expect(resolveTab('all', 'Maria').about).toBeUndefined();
    expect(resolveTab('maria', undefined).about).toBe('Maria');
  });

  it('turns a name into the matching tab, and me/partner into the default without a person', () => {
    expect(resolveTab('maria', 'Maria').value).toBe('me');
    expect(resolveTab('maria', 'Vitaliy').value).toBe('partner');
    expect(resolveTab('me', undefined).value).toBe('all');
  });

  it('keeps the settings under the section', () => {
    expect(settingsPath).toBe('/notes/settings');
  });
});
