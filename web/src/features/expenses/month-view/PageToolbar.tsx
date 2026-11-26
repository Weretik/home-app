import AddOutlinedIcon from '@mui/icons-material/AddOutlined';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import type { ReactNode } from 'react';

interface PageToolbarProps {
  /** What stands on the left: filters, a heading or a total. */
  children: ReactNode;
  /** The add button's text: «Добавить расход», «Добавить план». */
  addLabel: string;
  onAdd: () => void;
}

/** The row above a list: its filters or heading on the left, the add button on the right; wraps on a phone. */
export function PageToolbar({ children, addLabel, onAdd }: PageToolbarProps) {
  return (
    <Box
      sx={{
        display: 'flex',
        flexWrap: 'wrap',
        alignItems: 'center',
        justifyContent: 'space-between',
        gap: 2,
      }}
    >
      {children}
      <Button
        variant="contained"
        startIcon={<AddOutlinedIcon />}
        onClick={onAdd}
      >
        {addLabel}
      </Button>
    </Box>
  );
}
