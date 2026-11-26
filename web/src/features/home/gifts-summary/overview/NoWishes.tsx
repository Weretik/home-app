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
        Желаний пока нет
      </Typography>
      <Typography color="text.secondary">
        Добавьте подарки в разделе «Подарки».
      </Typography>
      <Button component={Link} to="/gifts">
        Открыть подарки
      </Button>
    </Card>
  );
}
