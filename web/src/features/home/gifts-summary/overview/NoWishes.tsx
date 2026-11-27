import CardGiftcardOutlinedIcon from '@mui/icons-material/CardGiftcardOutlined';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import Typography from '@mui/material/Typography';
import { Link } from 'react-router-dom';

/** Shown instead of the wishes and the occasions while nobody wants anything. */
export function NoWishes() {
  return (
    <Card
      sx={{
        p: 3,
        display: 'grid',
        justifyItems: 'center',
        textAlign: 'center',
        gap: 1,
      }}
    >
      <CardGiftcardOutlinedIcon color="primary" />
      <Typography variant="h6" component="p">
        Бажань поки немає
      </Typography>
      <Typography color="text.secondary">
        Додайте подарунки в розділі «Подарунки».
      </Typography>
      <Button component={Link} to="/gifts">
        Відкрити подарунки
      </Button>
    </Card>
  );
}
