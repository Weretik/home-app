import Tab from '@mui/material/Tab';
import Tabs from '@mui/material/Tabs';
import { Link, useLocation, useSearchParams } from 'react-router-dom';
import { expensesPages, pageUrl } from './expenses-pages';

/** Tabs between the section's pages. Switching keeps the month; filters belong to one page and are dropped. */
export function SectionTabs() {
  const { pathname } = useLocation();
  const [params] = useSearchParams();
  const current = pathname.replace(/\/$/, '');

  const keep = new URLSearchParams();
  for (const name of ['year', 'month']) {
    const value = params.get(name);
    if (value) keep.set(name, value);
  }
  const search = keep.size > 0 ? `?${keep}` : '';
  const active = expensesPages.find((page) => pageUrl(page) === current);

  return (
    <Tabs
      component="nav"
      aria-label="Страницы раздела"
      value={active ? pageUrl(active) : false}
      variant="scrollable"
      scrollButtons="auto"
      allowScrollButtonsMobile
      sx={{ borderBottom: 1, borderColor: 'divider', minHeight: 44 }}
    >
      {expensesPages.map((page) => (
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
  );
}
