import { describe, expect, it } from 'vitest';
import {
  formatNoteCount,
  kindLabels,
  kinds,
  noteFooter,
  personAbout,
  quickAddPrompt,
} from './labels';

describe('labels', () => {
  it('lists the kinds «Любить» first (FR-003)', () => {
    expect(kinds.map((kind) => kindLabels[kind])).toEqual([
      'Любить',
      'Не любить',
      'Факт',
    ]);
    expect(personAbout).toEqual({ Vitaliy: 'Віталія', Maria: 'Машу' });
  });

  it('asks the quick add about the partner by name and about me in the first person', () => {
    expect(quickAddPrompt('Likes', 'Maria', 'Vitaliy')).toBe('Що любить Маша?');
    expect(quickAddPrompt('Dislikes', 'Vitaliy', undefined)).toBe(
      'Що не любить Віталій?',
    );
    expect(quickAddPrompt('Fact', 'Maria', 'Vitaliy')).toBe(
      'Що варто знати про Машу?',
    );
    expect(quickAddPrompt('Likes', 'Maria', 'Maria')).toBe('Що я люблю?');
    expect(quickAddPrompt('Dislikes', 'Maria', 'Maria')).toBe('Що я не люблю?');
  });

  it('writes who wrote a note, when (Kyiv) and who changed it last (FR-004)', () => {
    const note = {
      createdBy: 'Maria' as const,
      updatedBy: 'Maria' as const,
      createdAtUtc: '2026-11-26T22:30:00Z',
    };
    expect(noteFooter(note)).toBe('Маша · 27.11');
    expect(noteFooter({ ...note, updatedBy: 'Vitaliy' })).toBe(
      'Маша · 27.11 · змінив Віталій',
    );
    expect(
      noteFooter({
        createdBy: 'Vitaliy',
        updatedBy: 'Maria',
        createdAtUtc: '2026-03-05T10:00:00Z',
      }),
    ).toBe('Віталій · 05.03 · змінила Маша');
    expect(noteFooter({ ...note, createdBy: null, updatedBy: null })).toBe(
      '27.11',
    );
  });

  it('counts notes with Ukrainian plurals', () => {
    expect(formatNoteCount(1)).toBe('1 нотатка');
    expect(formatNoteCount(3)).toBe('3 нотатки');
    expect(formatNoteCount(5)).toBe('5 нотаток');
    expect(formatNoteCount(21)).toBe('21 нотатка');
  });
});
