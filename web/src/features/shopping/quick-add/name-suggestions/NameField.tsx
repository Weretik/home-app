import Autocomplete from '@mui/material/Autocomplete';
import TextField from '@mui/material/TextField';
import type { NameSuggestion } from '../../data/shopping-api';
import { replaceSegment } from './name-segment';
import { SuggestionOption } from './SuggestionOption';
import { useSuggestionsFor } from './use-name-suggestions';

interface NameFieldProps {
  text: string;
  onTextChange: (text: string) => void;
}

const nameOf = (option: NameSuggestion | string) =>
  typeof option === 'string' ? option : option.name;

/**
 * «Что купить?» with suggestions for the segment being typed (FR-201…FR-206). The field is free text: it never holds a
 * chosen option, a choice only rewrites the current segment, and Enter without a highlighted option submits the form.
 */
export function NameField({ text, onTextChange }: NameFieldProps) {
  const options = useSuggestionsFor(text);

  return (
    <Autocomplete<NameSuggestion, false, false, true>
      freeSolo
      fullWidth
      size="small"
      sx={{ flex: 1, minWidth: 0 }}
      options={options}
      // The server already matched and ranked them.
      filterOptions={(found) => found}
      getOptionLabel={nameOf}
      getOptionKey={nameOf}
      value={null}
      inputValue={text}
      onInputChange={(_, value, reason) => {
        // A choice resets the input to the option's name: ignored, the choice rewrites the text itself.
        if (reason === 'input' || reason === 'clear') onTextChange(value);
      }}
      onChange={(_, value) => {
        if (value && typeof value !== 'string')
          onTextChange(replaceSegment(text, value.name));
      }}
      autoHighlight={false}
      clearOnBlur={false}
      selectOnFocus={false}
      handleHomeEndKeys={false}
      renderOption={({ key, ...props }, option) => (
        <SuggestionOption key={key} props={props} option={option} />
      )}
      renderInput={(params) => (
        <TextField
          {...params}
          placeholder="Что купить? молоко, хлеб, яйца 10"
          slotProps={{
            ...params.slotProps,
            htmlInput: {
              ...params.slotProps.htmlInput,
              'aria-label': 'Что купить',
              enterKeyHint: 'done',
            },
          }}
        />
      )}
    />
  );
}
