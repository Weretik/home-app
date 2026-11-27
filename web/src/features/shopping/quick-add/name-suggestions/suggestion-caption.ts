import { locale } from '../../../../platform/client/locale';
import type { NameSuggestion } from '../../data/shopping-api';

/** «брали 1 раз», «брали 3 рази», «брали 12 разів»; «ще не брали» for a name never bought. */
export function timesBoughtLabel(times: number): string {
  if (times <= 0) return 'ще не брали';
  const rule = new Intl.PluralRules(locale).select(times);
  const forms = { one: 'раз', few: 'рази', many: 'разів' } as const;
  return `брали ${times} ${forms[rule as keyof typeof forms] ?? forms.many}`;
}

/** The second line of a suggestion: «Молочное и яйца · брали 3 рази». */
export const suggestionCaption = (
  option: Pick<NameSuggestion, 'departmentName' | 'timesBought'>,
) => `${option.departmentName} · ${timesBoughtLabel(option.timesBought)}`;
