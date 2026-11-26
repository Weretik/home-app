import CheckCircleOutlinedIcon from '@mui/icons-material/CheckCircleOutlined';
import RadioButtonUncheckedOutlinedIcon from '@mui/icons-material/RadioButtonUncheckedOutlined';
import Checkbox from '@mui/material/Checkbox';

/** The round tick of a task row: one tap marks it done (FR-004). */
export function DoneCheckbox({
  title,
  checked,
  disabled,
  onCheck,
}: {
  title: string;
  checked: boolean;
  disabled?: boolean;
  onCheck: () => void;
}) {
  return (
    <Checkbox
      checked={checked}
      disabled={disabled}
      onChange={(event) => event.target.checked && onCheck()}
      icon={<RadioButtonUncheckedOutlinedIcon />}
      checkedIcon={<CheckCircleOutlinedIcon />}
      slotProps={{ input: { 'aria-label': `Готово: ${title}` } }}
      sx={{ alignSelf: 'flex-start', mt: -0.5 }}
    />
  );
}
