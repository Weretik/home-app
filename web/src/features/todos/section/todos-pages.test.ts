import { keptParams, pageUrl, settingsPath, todosPages } from './todos-pages';

test('the index page is the section itself, the archive is under it', () => {
  expect(todosPages.map(pageUrl)).toEqual(['/todos', '/todos/archive']);
  expect(settingsPath).toBe('/todos/settings');
});

test('a tab switch keeps the list filters', () => {
  expect(keptParams).toEqual(['who', 'category', 'q']);
});
