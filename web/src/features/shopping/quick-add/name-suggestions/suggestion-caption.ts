import type { NameSuggestion } from '../../data/shopping-api';

/** «брали 1 раз», «брали 3 раза», «брали 12 раз»; «ещё не брали» for a name never bought. */
export function timesBoughtLabel(times: number): string {
  if (times <= 0) return 'ещё не брали';
  const rule = new Intl.PluralRules('ru-RU').select(times);
  return `брали ${times} ${rule === 'few' ? 'раза' : 'раз'}`;
}

/** The second line of a suggestion: «Молочное и яйца · брали 3 раза». */
export const suggestionCaption = (
  option: Pick<NameSuggestion, 'departmentName' | 'timesBought'>,
) => `${option.departmentName} · ${timesBoughtLabel(option.timesBought)}`;
