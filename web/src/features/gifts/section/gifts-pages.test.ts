import { giftsPages, keptParams, pageUrl, settingsPath } from './gifts-pages';

test('the index page is the section itself, the archive is under it', () => {
  expect(giftsPages.map(pageUrl)).toEqual(['/gifts', '/gifts/archive']);
  expect(settingsPath).toBe('/gifts/settings');
});

test('a tab switch keeps the list filters only', () => {
  expect(keptParams).toEqual(['recipient', 'category']);
});
