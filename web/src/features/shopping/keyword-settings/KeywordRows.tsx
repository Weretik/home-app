import Chip from '@mui/material/Chip';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import { RowMenu } from '../page-parts/RowMenu';
import type { Keyword } from './keywords-api';

interface KeywordRowsProps {
  rows: Keyword[];
  onEdit: (keyword: Keyword) => void;
  onDelete: (keyword: Keyword) => void;
}

/** The keywords: the word, its department, «выучено» for the ones learned from a moved item, and «⋮». */
export function KeywordRows({ rows, onEdit, onDelete }: KeywordRowsProps) {
  return (
    <List aria-labelledby="keywords-title" dense>
      {rows.map((keyword) => (
        <ListItem
          key={keyword.id}
          divider
          secondaryAction={
            <RowMenu
              name={keyword.keyword}
              onEdit={() => onEdit(keyword)}
              onDelete={() => onDelete(keyword)}
            />
          }
        >
          <ListItemText
            primary={
              <>
                {keyword.keyword}
                {keyword.learned && (
                  <Chip size="small" label="выучено" sx={{ ml: 1 }} />
                )}
              </>
            }
            secondary={`${keyword.departmentIcon ?? ''} ${keyword.departmentName}`.trim()}
            slotProps={{ primary: { sx: { fontWeight: 600 } } }}
          />
        </ListItem>
      ))}
    </List>
  );
}
