import type { Keyword, KeywordRequest } from './keywords-api';

export interface KeywordValues {
  keyword: string;
  departmentId: string;
}

export const KEYWORD_MIN = 2;
export const KEYWORD_MAX = 60;

export const keywordValues = (
  keyword?: Keyword,
  departmentId = '',
): KeywordValues => ({
  keyword: keyword?.keyword ?? '',
  departmentId: keyword?.departmentId ?? departmentId,
});

/** Lower case, «ё» as «е», single spaces: the form the matcher compares (the API normalises the same way). */
export const normalizeKeyword = (text: string) =>
  text.toLowerCase().replaceAll('ё', 'е').replace(/\s+/g, ' ').trim();

/** The messages under the fields; empty when the form can be sent. */
export function checkKeyword(values: KeywordValues) {
  const errors: Partial<Record<keyof KeywordValues, string>> = {};
  const keyword = normalizeKeyword(values.keyword);
  if (!keyword) errors.keyword = 'Введіть слово';
  else if (keyword.length < KEYWORD_MIN)
    errors.keyword = `Не коротше ${KEYWORD_MIN} літер`;
  else if (keyword.length > KEYWORD_MAX)
    errors.keyword = `Не довше ${KEYWORD_MAX} символів`;
  if (!values.departmentId) errors.departmentId = 'Виберіть відділ';
  return errors;
}

export const keywordRequest = (values: KeywordValues): KeywordRequest => ({
  keyword: normalizeKeyword(values.keyword),
  departmentId: values.departmentId,
});
