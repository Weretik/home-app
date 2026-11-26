import { pageUrl, settingsPath, shoppingPages } from './shopping-pages';

test('the index page is the section itself, the history is under it', () => {
  expect(shoppingPages.map(pageUrl)).toEqual([
    '/shopping',
    '/shopping/history',
  ]);
  expect(settingsPath).toBe('/shopping/settings');
});
