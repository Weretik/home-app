import Box from '@mui/material/Box';
import { useState } from 'react';
import { personAbout } from '../data/labels';
import { useNotes, type NoteItem } from '../data/notes-api';
import { ListStatus } from '../feedback/ListStatus';
import { ResultSnackbar, type ResultMessage } from '../feedback/ResultSnackbar';
import { NoteDialog } from '../note-dialog/NoteDialog';
import type { NoteDraft } from '../note-dialog/note-form';
import { NoteFilters } from '../note-filters/NoteFilters';
import { isFiltered, quickAddKind, toNoteQuery } from '../url-state/filters';
import { useNoteFilters } from '../url-state/use-note-filters';
import { NoteGroups } from './NoteGroups';
import { QuickAdd } from './QuickAdd';

/** What the dialog shows: a note (by id, so it follows the list), a new one from its draft, or nothing. */
type Open = { id: string; note: NoteItem } | { draft: NoteDraft } | null;

/**
 * «Про нас»: the filters, the quick add and the tab's notes by category (FR-001…FR-004, FR-007). A tap opens the note
 * in the dialog. The list waits for the signed-in member, whose person picks the default tab.
 */
export function NotesPage() {
  const state = useNoteFilters();
  const { tab, filters, me } = state;
  const query = useNotes(toNoteQuery(tab.about, filters), !state.isLoading);
  const [open, setOpen] = useState<Open>(null);
  const [message, setMessage] = useState<ResultMessage | null>(null);
  const notes = query.data ?? [];
  // The open note as the list holds it now: after a 409 the list reloads, and the dialog gets the new version.
  const latest = (id: string, fallback: NoteItem) =>
    notes.find((note) => note.id === id) ?? fallback;

  const emptyTitle = !tab.about
    ? 'Тут ще нічого немає'
    : tab.about === me.person
      ? 'Про вас ще нічого немає'
      : `Про ${personAbout[tab.about]} ще нічого немає`;

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <NoteFilters state={state} />
      {!state.isLoading && (
        <QuickAdd
          key={tab.value}
          about={tab.about}
          me={me.person}
          startKind={quickAddKind(filters)}
          onOpenForm={(draft) => setOpen({ draft })}
        />
      )}
      <ListStatus
        isPending={query.isPending}
        error={query.error}
        isEmpty={notes.length === 0}
        isFiltered={isFiltered(filters)}
        emptyTitle={emptyTitle}
        onRetry={() => void query.refetch()}
        onResetFilters={state.clear}
      >
        <NoteGroups
          notes={notes}
          showPerson={!tab.about}
          onOpen={(note) => setOpen({ id: note.id, note })}
        />
      </ListStatus>
      {open && (
        <NoteDialog
          note={'id' in open ? latest(open.id, open.note) : undefined}
          draft={'draft' in open ? open.draft : undefined}
          onClose={() => setOpen(null)}
          onGone={() => {
            setOpen(null);
            setMessage({
              text: 'Цієї нотатки вже немає: її видалили в іншому місці.',
              severity: 'info',
            });
          }}
        />
      )}
      <ResultSnackbar message={message} onClose={() => setMessage(null)} />
    </Box>
  );
}
