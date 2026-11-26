import { useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { GIFT_PARAM, resolveGiftLink } from './gift-link';

const withoutLink = (current: URLSearchParams) => {
  const next = new URLSearchParams(current);
  next.delete(GIFT_PARAM);
  return next;
};

/**
 * Follows a link to one gift (`?gift=<id>`, web ADR 0013) once the tab's list is loaded: opens it with `open` and
 * drops the parameter (replace), or moves to «Подаренные» when the list does not hold it. A gift no tab holds keeps
 * the parameter and comes back as `missing` until `clearMissing` drops it.
 */
export function useGiftLink<T extends { id: string }>(
  archived: boolean,
  items: readonly T[] | undefined,
  open: (item: T) => void,
) {
  const [params, setParams] = useSearchParams();
  const navigate = useNavigate();
  const id = params.get(GIFT_PARAM);
  const step = id ? resolveGiftLink(id, items, archived) : undefined;
  const kind = step?.kind;
  const found = step?.kind === 'open' ? step.item : undefined;

  useEffect(() => {
    if (!id) return;
    if (kind === 'toArchive')
      navigate(`/gifts/archive?${GIFT_PARAM}=${encodeURIComponent(id)}`, {
        replace: true,
      });
    if (kind === 'open' && found) {
      open(found);
      setParams(withoutLink, { replace: true });
    }
  }, [id, kind, found, open, navigate, setParams]);

  return {
    missing: kind === 'missing',
    clearMissing: () => setParams(withoutLink, { replace: true }),
  };
}
