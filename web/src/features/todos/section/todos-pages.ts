/** The pages of the Todos section, shown as tabs (ADR 0006); «Настройки» is reached from the gear, not a tab. */
export interface TodosPage {
  /** Path under /todos; '' is the index page. */
  path: string;
  label: string;
}

export const todosPages: TodosPage[] = [
  { path: '', label: 'Задачи' },
  { path: 'archive', label: 'Архив' },
];

export const sectionPath = '/todos';

export const pageUrl = (page: TodosPage) =>
  page.path ? `${sectionPath}/${page.path}` : sectionPath;

export const settingsPath = `${sectionPath}/settings`;

/** The params a tab switch keeps: the filters work on both lists. */
export const keptParams = ['who', 'category', 'q'] as const;
