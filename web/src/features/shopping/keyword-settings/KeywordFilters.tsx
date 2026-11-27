import SearchOutlinedIcon from '@mui/icons-material/SearchOutlined';
import Box from '@mui/material/Box';
import InputAdornment from '@mui/material/InputAdornment';
import MenuItem from '@mui/material/MenuItem';
import TextField from '@mui/material/TextField';
import type { Department } from '../department-settings/departments-api';

interface KeywordFiltersProps {
  search: string;
  departmentId: string;
  departments: Department[];
  onSearch: (search: string) => void;
  onDepartment: (departmentId: string) => void;
}

/** Search by the start of a word and the department filter of «Слова для відділів». */
export function KeywordFilters(props: KeywordFiltersProps) {
  return (
    <Box
      sx={{
        display: 'grid',
        gap: 1.5,
        gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' },
        px: 2,
      }}
    >
      <TextField
        label="Пошук"
        size="small"
        value={props.search}
        onChange={(e) => props.onSearch(e.target.value)}
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
      <TextField
        select
        label="Відділ"
        size="small"
        value={props.departmentId}
        onChange={(e) => props.onDepartment(e.target.value)}
      >
        <MenuItem value="">Усі відділи</MenuItem>
        {props.departments.map((d) => (
          <MenuItem key={d.id} value={d.id}>
            {d.icon} {d.name}
          </MenuItem>
        ))}
      </TextField>
    </Box>
  );
}
