import ToggleButton from '@mui/material/ToggleButton';
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup';
import Typography from '@mui/material/Typography';
import Box from '@mui/material/Box';
import { personLabels } from '../data/labels';
import type { ShoppingPerson } from '../data/shopping-api';
import { useMe } from './use-me';

/** «Я: Віталій / Маша»: who this device adds and buys as; remembered on the device. */
export function MeToggle() {
  const [me, setMe] = useMe();

  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
      <Typography
        id="shopping-me"
        variant="body2"
        sx={{ color: 'text.secondary' }}
      >
        Я:
      </Typography>
      <ToggleButtonGroup
        exclusive
        size="small"
        value={me}
        aria-labelledby="shopping-me"
        onChange={(_, value: ShoppingPerson | null) => value && setMe(value)}
      >
        {(Object.keys(personLabels) as ShoppingPerson[]).map((person) => (
          <ToggleButton
            key={person}
            value={person}
            sx={{ textTransform: 'none', px: 1.5 }}
          >
            {personLabels[person]}
          </ToggleButton>
        ))}
      </ToggleButtonGroup>
    </Box>
  );
}
