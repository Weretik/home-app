import SettingsOutlinedIcon from '@mui/icons-material/SettingsOutlined';
import Box from '@mui/material/Box';
import IconButton from '@mui/material/IconButton';
import Skeleton from '@mui/material/Skeleton';
import Tab from '@mui/material/Tab';
import Tabs from '@mui/material/Tabs';
import Tooltip from '@mui/material/Tooltip';
import { Link, useLocation, useSearchParams } from 'react-router-dom';
import { useNoteFilters } from '../url-state/use-note-filters';
import { sectionPath, settingsPath, type NoteTab } from './notes-tabs';

/**
 * «Про мене», «Про Машу», «Усі» (or «Віталій», «Маша», «Усі») and the «Налаштування» gear at their end (FR-001).
 * Switching keeps the filters; while the member loads the tabs wait, so the default tab does not jump.
 */
export function NoteTabs() {
  const { pathname } = useLocation();
  const [params] = useSearchParams();
  const { tab, tabs, isLoading } = useNoteFilters();
  const onSettings = pathname.replace(/\/$/, '') === settingsPath;

  const href = (value: NoteTab) => {
    const next = new URLSearchParams(onSettings ? undefined : params);
    next.set('about', value);
    return `${sectionPath}?${next}`;
  };

  return (
    <Box
      sx={{
        display: 'flex',
        alignItems: 'center',
        gap: 1,
        borderBottom: 1,
        borderColor: 'divider',
        minWidth: 0,
      }}
    >
      {isLoading ? (
        <Skeleton
          aria-label="Завантажую вкладки"
          width={260}
          height={44}
          sx={{ flex: 1 }}
        />
      ) : (
        <Tabs
          component="nav"
          aria-label="Про кого"
          value={onSettings ? false : tab.value}
          variant="scrollable"
          scrollButtons="auto"
          allowScrollButtonsMobile
          sx={{ flex: 1, minWidth: 0, minHeight: 44 }}
        >
          {tabs.map((option) => (
            <Tab
              key={option.value}
              value={option.value}
              label={option.label}
              component={Link}
              to={href(option.value)}
              aria-current={!onSettings && option === tab ? 'page' : undefined}
              sx={{ minHeight: 44, textTransform: 'none', fontWeight: 600 }}
            />
          ))}
        </Tabs>
      )}
      <Tooltip title="Налаштування">
        <IconButton
          component={Link}
          to={settingsPath}
          aria-label="Налаштування"
          aria-current={onSettings ? 'page' : undefined}
          color={onSettings ? 'primary' : 'default'}
          sx={(theme) => ({
            bgcolor: onSettings
              ? `rgba(${theme.vars.palette.primary.mainChannel} / 0.12)`
              : undefined,
          })}
        >
          <SettingsOutlinedIcon />
        </IconButton>
      </Tooltip>
    </Box>
  );
}
