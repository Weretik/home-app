import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import List from '@mui/material/List';
import Typography from '@mui/material/Typography';
import { useId } from 'react';
import type { ShoppingItem } from '../data/shopping-api';
import { ItemRow } from './ItemRow';
import type { DepartmentGroup as Group } from './list-groups';

interface DepartmentGroupProps {
  group: Group;
  onToggle: (item: ShoppingItem) => void;
  onOpen: (item: ShoppingItem) => void;
}

/** One department of the list: its icon, name and count, then its items (R-3). */
export function DepartmentGroup({
  group,
  onToggle,
  onOpen,
}: DepartmentGroupProps) {
  const titleId = useId();
  return (
    <Card component="section" aria-labelledby={titleId}>
      <Typography
        id={titleId}
        variant="subtitle1"
        component="h2"
        sx={{ fontWeight: 600, px: 2, pt: 1.5 }}
      >
        {group.icon && (
          <Box component="span" aria-hidden sx={{ mr: 1 }}>
            {group.icon}
          </Box>
        )}
        {group.name}{' '}
        <Typography
          component="span"
          sx={{ color: 'text.secondary', fontWeight: 400 }}
        >
          · {group.items.length}
        </Typography>
      </Typography>
      <List aria-labelledby={titleId} sx={{ pt: 0.5 }}>
        {group.items.map((item, index) => (
          <ItemRow
            key={item.id}
            item={item}
            onToggle={onToggle}
            onOpen={onOpen}
            divider={index < group.items.length - 1}
          />
        ))}
      </List>
    </Card>
  );
}
