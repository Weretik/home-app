import { suggestionCaption, timesBoughtLabel } from './suggestion-caption';

test('says how many times a name was bought, in Russian plural', () => {
  expect(timesBoughtLabel(0)).toBe('ще не брали');
  expect(timesBoughtLabel(1)).toBe('брали 1 раз');
  expect(timesBoughtLabel(3)).toBe('брали 3 рази');
  expect(timesBoughtLabel(12)).toBe('брали 12 разів');
  expect(timesBoughtLabel(21)).toBe('брали 21 раз');
  expect(timesBoughtLabel(22)).toBe('брали 22 рази');
});

test('the caption names the department first', () => {
  expect(suggestionCaption({ departmentName: 'Хлеб', timesBought: 5 })).toBe(
    'Хлеб · брали 5 разів',
  );
});
