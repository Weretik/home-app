/** The ids in a new order with `id` moved one place up (-1) or down (+1); unchanged at the ends or for an unknown id. */
export function moveId(ids: string[], id: string, step: -1 | 1): string[] {
  const from = ids.indexOf(id);
  const to = from + step;
  if (from < 0 || to < 0 || to >= ids.length) return ids;
  const moved = [...ids];
  [moved[from], moved[to]] = [moved[to]!, moved[from]!];
  return moved;
}
