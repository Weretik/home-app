import Link from '@mui/material/Link';
import { Link as RouterLink } from 'react-router-dom';
import type { TodoTaskItem } from '../data/todos-api';

/**
 * «🎁 Відкрити подарунок» in the task dialog of a gift's to-do (gifts 002 FR-006): an in-app link to `/gifts?gift=<id>`
 * (web ADR 0013), never an import of the Gifts feature. Nothing for other tasks.
 */
export function GiftSourceLink({ task }: { task?: TodoTaskItem }) {
  if (task?.sourceKind !== 'gift' || !task.sourceId) return null;
  return (
    <Link
      component={RouterLink}
      to={`/gifts?gift=${encodeURIComponent(task.sourceId)}`}
      sx={{ justifySelf: 'start', fontWeight: 500 }}
    >
      🎁 Відкрити подарунок
    </Link>
  );
}
