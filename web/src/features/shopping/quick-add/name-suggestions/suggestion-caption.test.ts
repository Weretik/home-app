import { suggestionCaption, timesBoughtLabel } from './suggestion-caption';

test('says how many times a name was bought, in Russian plural', () => {
  expect(timesBoughtLabel(0)).toBe('ещё не брали');
  expect(timesBoughtLabel(1)).toBe('брали 1 раз');
  expect(timesBoughtLabel(3)).toBe('брали 3 раза');
  expect(timesBoughtLabel(12)).toBe('брали 12 раз');
  expect(timesBoughtLabel(21)).toBe('брали 21 раз');
  expect(timesBoughtLabel(22)).toBe('брали 22 раза');
});

test('the caption names the department first', () => {
  expect(suggestionCaption({ departmentName: 'Хлеб', timesBought: 5 })).toBe(
    'Хлеб · брали 5 раз',
  );
});
