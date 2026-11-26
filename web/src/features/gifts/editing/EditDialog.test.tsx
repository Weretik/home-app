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
        title="Новый подарок"
        error="Слишком много изменений подряд."
        isPending={false}
        onSubmit={onSubmit}
        onClose={vi.fn()}
      >
        <input aria-label="Название" />
      </EditDialog>,
    );
    expect(
      screen.getByRole('dialog', { name: 'Новый подарок' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('alert')).toHaveTextContent(
      'Слишком много изменений подряд.',
    );
    await userEvent.click(screen.getByRole('button', { name: 'Сохранить' }));
    expect(onSubmit).toHaveBeenCalledOnce();
  });

  it('locks both buttons while saving', () => {
    wrap(
      <EditDialog title="x" isPending onSubmit={vi.fn()} onClose={vi.fn()}>
        {null}
      </EditDialog>,
    );
    expect(screen.getByRole('button', { name: 'Сохраняю…' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Отмена' })).toBeDisabled();
  });
});

describe('DeleteDialog', () => {
  it('names the item and confirms with a red «Удалить»', async () => {
    const onConfirm = vi.fn();
    wrap(
      <DeleteDialog
        title="Удалить подарок?"
        summary="Серьги · Маша"
        isPending={false}
        onConfirm={onConfirm}
        onClose={vi.fn()}
      />,
    );
    expect(screen.getByText('Серьги · Маша')).toBeInTheDocument();
    await userEvent.click(screen.getByRole('button', { name: 'Удалить' }));
    expect(onConfirm).toHaveBeenCalledOnce();
  });

  it('shows why a delete failed and locks while deleting', () => {
    wrap(
      <DeleteDialog
        title="Удалить категорию?"
        summary="Книги"
        error="Эта категория есть у подарков"
        isPending
        onConfirm={vi.fn()}
        onClose={vi.fn()}
      />,
    );
    expect(screen.getByRole('alert')).toHaveTextContent(
      'Эта категория есть у подарков',
    );
    expect(screen.getByRole('button', { name: 'Удаляю…' })).toBeDisabled();
  });
});
