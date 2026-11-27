import AddOutlinedIcon from '@mui/icons-material/AddOutlined';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import FormHelperText from '@mui/material/FormHelperText';
import Typography from '@mui/material/Typography';
import { useRef, useState } from 'react';
import type { FieldErrors } from '../editing/field-checks';
import {
  MAX_VARIANTS,
  newVariant,
  type VariantValues,
} from '../gift-editing/gift-form';
import { VariantRow } from './VariantRow';

interface VariantsEditorProps {
  variants: VariantValues[];
  errors: FieldErrors;
  onChange: (variants: VariantValues[]) => void;
}

/** The variants of a gift: rows with a link, title and price; add up to 20, remove any. */
export function VariantsEditor({
  variants,
  errors,
  onChange,
}: VariantsEditorProps) {
  const [added, setAdded] = useState<string>();
  const addButton = useRef<HTMLButtonElement>(null);
  const isFull = variants.length >= MAX_VARIANTS;

  const add = () => {
    const variant = newVariant();
    setAdded(variant.key);
    onChange([...variants, variant]);
  };
  const remove = (key: string) => {
    onChange(variants.filter((variant) => variant.key !== key));
    addButton.current?.focus();
  };
  const patch = (key: string, change: Partial<VariantValues>) =>
    onChange(
      variants.map((variant) =>
        variant.key === key ? { ...variant, ...change } : variant,
      ),
    );

  return (
    <Box
      component="fieldset"
      sx={{ border: 0, m: 0, p: 0, display: 'grid', gap: 1.5 }}
    >
      <Typography component="legend" variant="subtitle2" sx={{ mb: 1 }}>
        Варіанти
      </Typography>
      {variants.length > 0 && (
        <Box
          component="ul"
          sx={{ m: 0, p: 0, listStyle: 'none', display: 'grid', gap: 1.5 }}
        >
          {variants.map((variant, index) => (
            <VariantRow
              key={variant.key}
              variant={variant}
              index={index}
              errors={errors}
              focusLink={variant.key === added}
              onChange={(change) => patch(variant.key, change)}
              onRemove={() => remove(variant.key)}
            />
          ))}
        </Box>
      )}
      <Box>
        <Button
          ref={addButton}
          startIcon={<AddOutlinedIcon />}
          onClick={add}
          disabled={isFull}
        >
          Додати варіант
        </Button>
        {isFull && (
          <FormHelperText>Не більше {MAX_VARIANTS} варіантів</FormHelperText>
        )}
        {errors.variants && (
          <FormHelperText error>{errors.variants}</FormHelperText>
        )}
      </Box>
    </Box>
  );
}
