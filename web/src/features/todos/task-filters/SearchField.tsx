import SearchOutlinedIcon from '@mui/icons-material/SearchOutlined';
import InputAdornment from '@mui/material/InputAdornment';
import TextField from '@mui/material/TextField';
import { useDebouncedSearch } from '../url-state/use-todo-filters';

/** Search by words in the title; the URL follows 300 ms after typing stops (FR-002). */
export function SearchField({
  value,
  onChange,
}: {
  value: string | undefined;
  onChange: (value: string | undefined) => void;
}) {
  const [text, setText] = useDebouncedSearch(value, onChange);
  return (
    <TextField
      size="small"
      type="search"
      label="Поиск"
      value={text}
      onChange={(event) => setText(event.target.value)}
      sx={{ minWidth: 0, flex: { xs: '1 1 100%', sm: '0 1 240px' } }}
      slotProps={{
        input: {
          startAdornment: (
            <InputAdornment position="start">
              <SearchOutlinedIcon fontSize="small" />
            </InputAdornment>
          ),
        },
      }}
    />
  );
}
