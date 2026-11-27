/** The card grid (FR-004, docs/design.md «Pages»): one column on a phone, two from `sm`, three from `lg`. */
export const cardGrid = {
  display: 'grid',
  gap: { xs: 2, md: 3 },
  gridTemplateColumns: {
    xs: 'minmax(0, 1fr)',
    sm: 'repeat(2, minmax(0, 1fr))',
    lg: 'repeat(3, minmax(0, 1fr))',
  },
} as const;
