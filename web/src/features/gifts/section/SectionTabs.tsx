import SettingsOutlinedIcon from '@mui/icons-material/SettingsOutlined';
import Box from '@mui/material/Box';
import IconButton from '@mui/material/IconButton';
import Tab from '@mui/material/Tab';
import Tabs from '@mui/material/Tabs';
import Tooltip from '@mui/material/Tooltip';
import { Link, useLocation, useSearchParams } from 'react-router-dom';
import { giftsPages, keptParams, pageUrl, settingsPath } from './gifts-pages';

/** «Список» and «Подаренные» with the filters kept, and the «Настройки» gear at their end (without filters). */
export function SectionTabs() {
  const { pathname } = useLocation();
  const [params] = useSearchParams();
  const current = pathname.replace(/\/$/, '');

  const keep = new URLSearchParams();
  for (const name of keptParams) {
    const value = params.get(name);
    if (value) keep.set(name, value);
  }
  const search = keep.size > 0 ? `?${keep}` : '';
  const active = giftsPages.find((page) => pageUrl(page) === current);
  const onSettings = current === settingsPath;

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
      <Tabs
        component="nav"
        aria-label="Страницы раздела"
        value={active ? pageUrl(active) : false}
        variant="scrollable"
        scrollButtons="auto"
        allowScrollButtonsMobile
        sx={{ flex: 1, minWidth: 0, minHeight: 44 }}
      >
        {giftsPages.map((page) => (
          <Tab
            key={page.path}
            value={pageUrl(page)}
            label={page.label}
            component={Link}
            to={`${pageUrl(page)}${search}`}
            aria-current={active === page ? 'page' : undefined}
            sx={{ minHeight: 44, textTransform: 'none', fontWeight: 600 }}
          />
        ))}
      </Tabs>
      <Tooltip title="Настройки">
        <IconButton
          component={Link}
          to={settingsPath}
          aria-label="Настройки"
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
