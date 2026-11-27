import AddOutlinedIcon from '@mui/icons-material/AddOutlined';
import Box from '@mui/material/Box';
import IconButton from '@mui/material/IconButton';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import Tooltip from '@mui/material/Tooltip';
import Typography from '@mui/material/Typography';
import {
  useGiftDetails,
  type GiftDetailItem,
  type Recipient,
} from '../data/gifts-api';
import { recipientLabels, recipients } from '../display/labels';
import { DetailEditor } from '../detail-editing/DetailEditor';
import { RowActions } from '../editing/RowActions';
import { useEditorState } from '../editing/use-editor-state';
import { SettingsCard } from './SettingsCard';

/** «Нюанси»: sizes and other details per person, edited only here; the home summary shows them read-only. */
export function DetailsCard() {
  const details = useGiftDetails();
  const editor = useEditorState<GiftDetailItem, Recipient>();
  const all = details.data ?? [];
  const groups = recipients
    .map((recipient) => ({
      recipient,
      items: all.filter((d) => d.recipient === recipient),
    }))
    .filter((group) => group.items.length > 0);

  return (
    <SettingsCard
      title="Нюанси"
      caption="Розміри та деталі, які важливо пам’ятати: каблучка, одяг, взуття."
      addLabel="Додати нюанс"
      onAdd={() => editor.add()}
      isPending={details.isPending}
      error={details.error}
      onRetry={() => void details.refetch()}
    >
      {groups.length === 0 && (
        <Typography color="text.secondary" sx={{ py: 1 }}>
          Нюансів поки немає. Додайте, наприклад, «Каблучка — 18,5–19,0» для
          Маші.
        </Typography>
      )}
      {groups.map((group) => {
        const name = recipientLabels[group.recipient];
        return (
          <Box key={group.recipient} sx={{ mt: 1 }}>
            <Box
              sx={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
              }}
            >
              <Typography variant="subtitle2" component="h3">
                {name}
              </Typography>
              <Tooltip title={`Додати нюанс для ${name}`}>
                <IconButton
                  size="small"
                  aria-label={`Додати нюанс для ${name}`}
                  onClick={() => editor.add(group.recipient)}
                >
                  <AddOutlinedIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            </Box>
            <List aria-label={name} dense disablePadding>
              {group.items.map((detail) => (
                <ListItem
                  key={detail.id}
                  disableGutters
                  divider
                  sx={{ gap: 2 }}
                >
                  <Typography
                    sx={{ flex: 1, minWidth: 0, overflowWrap: 'anywhere' }}
                  >
                    {detail.label}
                  </Typography>
                  <Typography
                    sx={{
                      fontWeight: 600,
                      overflowWrap: 'anywhere',
                      textAlign: 'right',
                    }}
                  >
                    {detail.value}
                  </Typography>
                  <RowActions
                    label={detail.label}
                    onEdit={() => editor.edit(detail)}
                    onDelete={() => editor.remove(detail)}
                  />
                </ListItem>
              ))}
            </List>
          </Box>
        );
      })}
      <DetailEditor state={editor.state} onClose={editor.close} />
    </SettingsCard>
  );
}
