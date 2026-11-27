import FavoriteBorderIcon from '@mui/icons-material/FavoriteBorder';
import LogoutIcon from '@mui/icons-material/Logout';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Divider from '@mui/material/Divider';
import Skeleton from '@mui/material/Skeleton';
import Typography from '@mui/material/Typography';
import { Link as RouterLink } from 'react-router-dom';
import type { FamilyPerson } from '../../../platform/client/member-api';
import { useCurrentMember, useSignOut } from '../data/identity-api';
import { MemberAvatar } from '../member/MemberAvatar';
import { TelegramRow } from './TelegramRow';

const personLabels: Record<FamilyPerson, string> = {
  Vitaliy: 'Віталій',
  Maria: 'Маша',
};

/**
 * «Профіль» (identity 002 FR-004): the Google picture and name, the e-mail, who the member is in the home, the
 * Telegram chat, the link to «Про нас» (notes 001 FR-008) and «Вийти». Links to other sections go after Telegram.
 */
export function ProfilePage() {
  const { data: member } = useCurrentMember();
  const signOut = useSignOut();
  const person = member?.person ?? undefined;

  return (
    <Box
      sx={{
        display: 'grid',
        gap: { xs: 2, md: 3 },
        minWidth: 0,
        maxWidth: 640,
      }}
    >
      <Typography variant="h4" component="h1">
        Профіль
      </Typography>
      <Card>
        <CardContent sx={{ display: 'grid', gap: 2 }}>
          <Box
            sx={{ display: 'flex', alignItems: 'center', gap: 2, minWidth: 0 }}
          >
            <MemberAvatar member={member} size={72} />
            <Box sx={{ minWidth: 0 }}>
              <Typography
                variant="h6"
                component="p"
                sx={{ overflowWrap: 'anywhere' }}
              >
                {member ? member.name : <Skeleton width={160} />}
              </Typography>
              <Typography
                sx={{ color: 'text.secondary', overflowWrap: 'anywhere' }}
              >
                {member ? member.email : <Skeleton width={200} />}
              </Typography>
            </Box>
          </Box>
          <Divider />
          <Box>
            <Typography variant="body2" sx={{ color: 'text.secondary' }}>
              У домі
            </Typography>
            <Typography>
              {person ? (
                personLabels[person]
              ) : member ? (
                'Гість'
              ) : (
                <Skeleton width={80} />
              )}
            </Typography>
          </Box>
          <Divider />
          <TelegramRow person={person} />
          <Divider />
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              gap: 2,
              flexWrap: 'wrap',
            }}
          >
            <Box sx={{ flex: '1 1 200px', minWidth: 0 }}>
              <Typography>Про нас</Typography>
              <Typography variant="body2" sx={{ color: 'text.secondary' }}>
                Що ми любимо, не любимо й варто знати одне про одного
              </Typography>
            </Box>
            <Button
              component={RouterLink}
              to="/notes"
              variant="outlined"
              startIcon={<FavoriteBorderIcon />}
            >
              Відкрити
            </Button>
          </Box>
        </CardContent>
      </Card>
      <Box>
        <Button
          variant="outlined"
          color="inherit"
          startIcon={<LogoutIcon />}
          disabled={signOut.isPending}
          onClick={() => signOut.mutate()}
        >
          Вийти
        </Button>
      </Box>
    </Box>
  );
}
