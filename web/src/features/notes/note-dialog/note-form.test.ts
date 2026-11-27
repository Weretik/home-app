import { describe, expect, it } from 'vitest';
import type { NoteItem } from '../data/notes-api';
import {
  isValid,
  noteFormValues,
  quickNote,
  toNoteRequest,
  validateNote,
} from './note-form';

const note = {
  id: 'n1',
  about: 'Maria',
  kind: 'Dislikes',
  title: 'Томатний сік',
  details: null,
  place: 'Сільпо',
  isImportant: true,
  categoryId: 'c1',
  categoryName: 'Їжа',
  categoryEmoji: null,
  createdBy: 'Vitaliy',
  updatedBy: 'Vitaliy',
  createdAtUtc: '2026-11-27T10:00:00Z',
  updatedAtUtc: '2026-11-27T10:00:00Z',
  version: 3,
} satisfies NoteItem;

describe('note form', () => {
  it('starts a new note from its draft and an existing one from the note', () => {
    expect(
      noteFormValues(undefined, { about: 'Maria', kind: 'Fact', title: 'x' }),
    ).toEqual({
      about: 'Maria',
      kind: 'Fact',
      title: 'x',
      categoryId: '',
      place: '',
      details: '',
      isImportant: false,
    });
    expect(noteFormValues(note)).toMatchObject({
      categoryId: 'c1',
      place: 'Сільпо',
      details: '',
      isImportant: true,
    });
  });

  it('checks the title, the place and the description (R-003…R-005)', () => {
    const values = noteFormValues(note);
    expect(validateNote({ ...values, title: ' ' }).title).toBe(
      'Введіть, що саме',
    );
    expect(validateNote({ ...values, title: 'x'.repeat(201) }).title).toMatch(
      /200/,
    );
    expect(validateNote({ ...values, place: 'x'.repeat(201) }).place).toMatch(
      /200/,
    );
    expect(
      validateNote({ ...values, details: 'x'.repeat(2001) }).details,
    ).toMatch(/2000/);
    expect(isValid(validateNote(values))).toBe(true);
  });

  it('trims the text and sends empty fields as null', () => {
    expect(
      toNoteRequest({
        ...noteFormValues(note),
        title: ' Сік ',
        place: ' ',
        details: ' пише ',
        categoryId: '',
      }),
    ).toEqual({
      about: 'Maria',
      kind: 'Dislikes',
      title: 'Сік',
      place: null,
      details: 'пише',
      categoryId: null,
      isImportant: true,
    });
  });

  it('makes a quick note without a category (FR-003)', () => {
    expect(
      quickNote({ about: 'Vitaliy', kind: 'Likes', title: ' Каву ' }),
    ).toEqual({
      about: 'Vitaliy',
      kind: 'Likes',
      title: 'Каву',
      details: null,
      place: null,
      categoryId: null,
      isImportant: false,
    });
  });
});
