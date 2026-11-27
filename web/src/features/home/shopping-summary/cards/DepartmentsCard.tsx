import Card from '@mui/material/Card';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import Typography from '@mui/material/Typography';
import type { ShoppingSummary } from '../data/shopping-summary-api';

/** «За відділами»: departments with needed items in store order and their counts. */
export function DepartmentsCard({ summary }: { summary: ShoppingSummary }) {
  return (
    <Card component="section" aria-labelledby="shopping-departments-title">
      <Typography
        id="shopping-departments-title"
        variant="subtitle1"
        component="h2"
        sx={{ fontWeight: 600, px: 2, pt: 2 }}
      >
        За відділами
      </Typography>
      <List aria-labelledby="shopping-departments-title" dense>
        {summary.departments.map((department) => (
          <ListItem
            key={department.departmentId}
            secondaryAction={
              <Typography
                sx={{ fontWeight: 600, fontVariantNumeric: 'tabular-nums' }}
              >
                {department.neededCount}
              </Typography>
            }
          >
            <ListItemText
              primary={`${department.icon ? `${department.icon} ` : ''}${department.name}`}
            />
          </ListItem>
        ))}
      </List>
    </Card>
  );
}
