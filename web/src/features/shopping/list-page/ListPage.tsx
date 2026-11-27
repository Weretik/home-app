import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import { useState } from 'react';
import { describeWriteError } from '../data/write-failure-api';
import { useShoppingList, type ShoppingItem } from '../data/shopping-api';
import { useSetItemStatus } from '../data/status-api';
import { ItemDialog } from '../item-editing/ItemDialog';
import { PageStatus } from '../page-parts/PageStatus';
import { QuickAdd } from '../quick-add/entry/QuickAdd';
import { UndoSnackbar } from '../quick-add/entry/UndoSnackbar';
import { useQuickAdd } from '../quick-add/entry/use-quick-add';
import { useMe } from '../section/use-me';
import { CartCard } from './CartCard';
import { DepartmentGroup } from './DepartmentGroup';
import { cartItems, groupNeeded } from './list-groups';

/** «Список»: the quick add, the needed items by department in store order, the cart (R-1…R-8, R-15). */
export function ListPage() {
  const list = useShoppingList();
  const quickAdd = useQuickAdd();
  const setStatus = useSetItemStatus();
  const [me] = useMe();
  const [editing, setEditing] = useState<ShoppingItem | null>(null);

  const items = list.data?.items ?? [];
  const groups = groupNeeded(items);
  const cart = cartItems(items);
  const toggle = (item: ShoppingItem) =>
    setStatus.mutate({
      id: item.id,
      status: item.status === 'Needed' ? 'Bought' : 'Needed',
      by: me,
    });

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <QuickAdd quickAdd={quickAdd} />
      {setStatus.error && (
        <Alert severity="error" onClose={() => setStatus.reset()}>
          {describeWriteError(setStatus.error, 'позначити').message}
        </Alert>
      )}
      <PageStatus
        isPending={list.isPending}
        error={list.error}
        hasData={list.data !== undefined}
        isEmpty={items.length === 0}
        loadingLabel="Завантажую список"
        empty={{
          emoji: '🛒',
          title: 'Список порожній 🎉',
          hint: 'Напишіть у полі вище, що купити: можна одразу кілька через кому, або натисніть на ⭐ часте.',
        }}
        onRetry={() => void list.refetch()}
      >
        <Box
          sx={{
            display: 'grid',
            gap: { xs: 2, md: 3 },
            gridTemplateColumns: { xs: '1fr', lg: '1fr 1fr' },
            alignItems: 'start',
          }}
        >
          {groups.map((group) => (
            <DepartmentGroup
              key={group.departmentId}
              group={group}
              onToggle={toggle}
              onOpen={setEditing}
            />
          ))}
        </Box>
        <CartCard items={cart} onToggle={toggle} onOpen={setEditing} />
      </PageStatus>
      {editing && (
        <ItemDialog item={editing} onClose={() => setEditing(null)} />
      )}
      <UndoSnackbar quickAdd={quickAdd} />
    </Box>
  );
}
