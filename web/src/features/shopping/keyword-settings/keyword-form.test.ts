import {
  checkKeyword,
  KEYWORD_MAX,
  KEYWORD_MIN,
  keywordRequest,
  keywordValues,
  normalizeKeyword,
} from './keyword-form';

test('starts from the chosen department for a new keyword', () => {
  expect(keywordValues(undefined, 'd1')).toEqual({
    keyword: '',
    departmentId: 'd1',
  });
});

test('normalises like the API: lower case, ё as е, single spaces', () => {
  expect(normalizeKeyword('  Зелёный   ЧАЙ ')).toBe('зеленый чай');
});

test('needs a keyword of the right length and a department', () => {
  expect(checkKeyword({ keyword: ' ', departmentId: '' })).toEqual({
    keyword: 'Введіть слово',
    departmentId: 'Виберіть відділ',
  });
  expect(checkKeyword({ keyword: 'я', departmentId: 'd1' })).toEqual({
    keyword: `Не коротше ${KEYWORD_MIN} літер`,
  });
  expect(
    checkKeyword({ keyword: 'я'.repeat(KEYWORD_MAX + 1), departmentId: 'd1' }),
  ).toEqual({ keyword: `Не довше ${KEYWORD_MAX} символів` });
  expect(checkKeyword({ keyword: 'чай', departmentId: 'd1' })).toEqual({});
});

test('sends the normalised keyword', () => {
  expect(keywordRequest({ keyword: ' Ёжик ', departmentId: 'd1' })).toEqual({
    keyword: 'ежик',
    departmentId: 'd1',
  });
});
