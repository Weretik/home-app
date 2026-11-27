import Card from '@mui/material/Card';
import type { Theme } from '@mui/material/styles';
import useMediaQuery from '@mui/material/useMediaQuery';
import type { ReactNode } from 'react';
import { ListStatus, type ListStatusProps } from './ListStatus';
import { LoadMoreButton } from './LoadMoreButton';

/** What the card needs of a list loaded page by page (`useInfiniteQuery`). */
interface PagedQuery<Item> {
  data?: { pages: { items: Item[]; totalCount: number }[] };
  isPending: boolean;
  error: Error | null;
  hasNextPage: boolean;
  isFetchingNextPage: boolean;
  refetch: () => unknown;
  fetchNextPage: () => unknown;
}

type StatusProps = Omit<
  ListStatusProps,
  'isPending' | 'error' | 'isEmpty' | 'onRetry' | 'children'
>;

interface PagedListCardProps<Item> extends StatusProps {
  query: PagedQuery<Item>;
  /** The rows: a compact list on a phone, a table from `md`. */
  children: (items: Item[], isComputer: boolean) => ReactNode;
}

/** A month list in a card: loading, error and empty states, the rows, and «Показати ще» while pages remain. */
export function PagedListCard<Item>({
  query,
  children,
  ...status
}: PagedListCardProps<Item>) {
  const isComputer = useMediaQuery((theme: Theme) =>
    theme.breakpoints.up('md'),
  );
  const pages = query.data?.pages ?? [];
  const items = pages.flatMap((page) => page.items);
  const first = pages[0];

  return (
    <Card>
      <ListStatus
        {...status}
        isPending={query.isPending}
        error={query.error}
        isEmpty={items.length === 0}
        onRetry={() => void query.refetch()}
      >
        {children(items, isComputer)}
        {query.hasNextPage && first && (
          <LoadMoreButton
            shown={items.length}
            total={first.totalCount}
            isLoading={query.isFetchingNextPage}
            onClick={() => void query.fetchNextPage()}
          />
        )}
      </ListStatus>
    </Card>
  );
}
