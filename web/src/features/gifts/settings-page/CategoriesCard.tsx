import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import Typography from '@mui/material/Typography';
import { CategoryEditor } from '../category-editing/CategoryEditor';
import { useGiftCategories, type GiftCategoryItem } from '../data/gifts-api';
import { RowActions } from '../editing/RowActions';
import { useEditorState } from '../editing/use-editor-state';
import { SettingsCard } from './SettingsCard';

/** «Категории»: add, rename, delete; a category that gifts use is refused by the server and the dialog says why. */
export function CategoriesCard() {
  const categories = useGiftCategories();
  const editor = useEditorState<GiftCategoryItem>();
  const all = categories.data ?? [];

  return (
    <SettingsCard
      title="Категории"
      caption="По ним фильтруется список подарков."
      addLabel="Добавить категорию"
      onAdd={() => editor.add()}
      isPending={categories.isPending}
      error={categories.error}
      onRetry={() => void categories.refetch()}
    >
      {all.length === 0 ? (
        <Typography color="text.secondary" sx={{ py: 1 }}>
          Категорий пока нет. Добавьте первую, например «Украшения».
        </Typography>
      ) : (
        <List aria-label="Категории" dense disablePadding>
          {all.map((category, index) => (
            <ListItem
              key={category.id}
              disableGutters
              divider={index < all.length - 1}
            >
              <Typography
                sx={{ flex: 1, minWidth: 0, overflowWrap: 'anywhere' }}
              >
                {category.name}
              </Typography>
              <RowActions
                label={category.name}
                onEdit={() => editor.edit(category)}
                onDelete={() => editor.remove(category)}
              />
            </ListItem>
          ))}
        </List>
      )}
      <CategoryEditor state={editor.state} onClose={editor.close} />
    </SettingsCard>
  );
}
