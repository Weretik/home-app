import { ThemeProvider } from '@mui/material/styles';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { DeleteDialog } from './DeleteDialog';
import { EditDialog } from './EditDialog';

const wrap = (node: React.ReactNode) =>
  render(<ThemeProvider theme={theme}>{node}</ThemeProvider>);

describe('EditDialog', () => {
  it('submits the form and shows a failed save above the fields', async () => {
    const onSubmit = vi.fn();
    wrap(
      <EditDialog
        title="Новий подарунок"
        error="Забагато змін поспіль."
        isPending={false}
        onSubmit={onSubmit}
        onClose={vi.fn()}
      >
        <input aria-label="Назва" />
      </EditDialog>,
    );
    expect(
      screen.getByRole('dialog', { name: 'Новий подарунок' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('alert')).toHaveTextContent(
      'Забагато змін поспіль.',
    );
    await userEvent.click(screen.getByRole('button', { name: 'Зберегти' }));
    expect(onSubmit).toHaveBeenCalledOnce();
  });

  it('locks both buttons while saving', () => {
    wrap(
      <EditDialog title="x" isPending onSubmit={vi.fn()} onClose={vi.fn()}>
        {null}
      </EditDialog>,
    );
    expect(screen.getByRole('button', { name: 'Зберігаю…' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Скасувати' })).toBeDisabled();
  });
});

describe('DeleteDialog', () => {
  it('names the item and confirms with a red «Видалити»', async () => {
    const onConfirm = vi.fn();
    wrap(
      <DeleteDialog
        title="Видалити подарунок?"
        summary="Серьги · Маша"
        isPending={false}
        onConfirm={onConfirm}
        onClose={vi.fn()}
      />,
    );
    expect(screen.getByText('Серьги · Маша')).toBeInTheDocument();
    await userEvent.click(screen.getByRole('button', { name: 'Видалити' }));
    expect(onConfirm).toHaveBeenCalledOnce();
  });

  it('shows why a delete failed and locks while deleting', () => {
    wrap(
      <DeleteDialog
        title="Видалити категорію?"
        summary="Книги"
        error="Ця категорія є в подарунків"
        isPending
        onConfirm={vi.fn()}
        onClose={vi.fn()}
      />,
    );
    expect(screen.getByRole('alert')).toHaveTextContent(
      'Ця категорія є в подарунків',
    );
    expect(screen.getByRole('button', { name: 'Видаляю…' })).toBeDisabled();
  });
});
