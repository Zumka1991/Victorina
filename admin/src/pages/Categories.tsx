import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getCategories, createCategory, updateCategory, deleteCategory, getCategoryTranslations } from '../services/api';
import type { Category } from '../types';
import { SUPPORTED_LANGUAGES } from '../types';

export default function Categories() {
  const queryClient = useQueryClient();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingCategory, setEditingCategory] = useState<Category | null>(null);
  const [translatingFrom, setTranslatingFrom] = useState<Category | null>(null);
  const [translationGroup, setTranslationGroup] = useState<Category[]>([]);
  const [form, setForm] = useState({
    name: '',
    description: '',
    emoji: '',
    languageCode: 'ru',
    translationGroupId: undefined as string | undefined
  });

  const { data: categories, isLoading } = useQuery({
    queryKey: ['categories'],
    queryFn: getCategories,
  });

  const createMutation = useMutation({
    mutationFn: createCategory,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      closeModal();
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Omit<Category, 'id'> }) =>
      updateCategory(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      closeModal();
    },
  });

  const deleteMutation = useMutation({
    mutationFn: deleteCategory,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
    },
  });

  const openModal = async (category?: Category) => {
    if (category) {
      setEditingCategory(category);
      setTranslatingFrom(null);
      setForm({
        name: category.name,
        description: category.description || '',
        emoji: category.emoji || '',
        languageCode: category.languageCode || 'ru',
        translationGroupId: category.translationGroupId,
      });
      // Load all translations for this category
      if (category.translationGroupId) {
        try {
          const translations = await getCategoryTranslations(category.translationGroupId);
          setTranslationGroup(translations);
        } catch {
          setTranslationGroup([category]);
        }
      } else {
        setTranslationGroup([category]);
      }
    } else {
      setEditingCategory(null);
      setTranslatingFrom(null);
      setTranslationGroup([]);
      setForm({
        name: '',
        description: '',
        emoji: '',
        languageCode: 'ru',
        translationGroupId: undefined
      });
    }
    setIsModalOpen(true);
  };

  // Open modal to add translation for existing category
  const openTranslationModal = (category: Category, targetLang: string) => {
    setEditingCategory(null);
    setTranslatingFrom(category);
    setTranslationGroup([category]);
    setForm({
      name: '',
      description: '',
      emoji: category.emoji || '', // Keep same emoji for linked categories
      languageCode: targetLang,
      translationGroupId: category.translationGroupId,
    });
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setEditingCategory(null);
    setTranslatingFrom(null);
    setTranslationGroup([]);
    setForm({
      name: '',
      description: '',
      emoji: '',
      languageCode: 'ru',
      translationGroupId: undefined
    });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (editingCategory) {
      updateMutation.mutate({ id: editingCategory.id, data: form });
    } else {
      createMutation.mutate(form);
    }
  };

  // Get existing translations for a category
  const getExistingTranslations = (category: Category): string[] => {
    if (!category.translationGroupId || !categories) return [category.languageCode];

    const translations = categories
      .filter(c => c.translationGroupId === category.translationGroupId)
      .map(c => c.languageCode);

    return [...new Set(translations)];
  };

  // Get missing languages for translation
  const getMissingLanguages = (category: Category): typeof SUPPORTED_LANGUAGES[number][] => {
    const existing = getExistingTranslations(category);
    return SUPPORTED_LANGUAGES.filter(lang => !existing.includes(lang.code));
  };

  // Group categories by TranslationGroupId for display
  const groupedCategories = () => {
    if (!categories) return [];

    const groups = new Map<string | undefined, Category[]>();
    const ungrouped: Category[] = [];

    categories.forEach(cat => {
      if (cat.translationGroupId) {
        const existing = groups.get(cat.translationGroupId) || [];
        existing.push(cat);
        groups.set(cat.translationGroupId, existing);
      } else {
        ungrouped.push(cat);
      }
    });

    // Return first category of each group + ungrouped
    const result: Category[] = [];
    groups.forEach(group => {
      // Sort by languageCode to show consistently
      group.sort((a, b) => {
        const aIdx = SUPPORTED_LANGUAGES.findIndex(l => l.code === a.languageCode);
        const bIdx = SUPPORTED_LANGUAGES.findIndex(l => l.code === b.languageCode);
        return aIdx - bIdx;
      });
      result.push(group[0]);
    });
    result.push(...ungrouped);

    return result;
  };

  if (isLoading) {
    return <div>Загрузка...</div>;
  }

  const displayCategories = groupedCategories();

  return (
    <div>
      <div className="page-header">
        <h2>📁 Категории</h2>
        <button className="btn btn-primary" onClick={() => openModal()}>
          ➕ Добавить категорию
        </button>
      </div>

      <div className="card">
        {displayCategories && displayCategories.length > 0 ? (
          <table>
            <thead>
              <tr>
                <th>Emoji</th>
                <th>Название</th>
                <th>Переводы</th>
                <th>Описание</th>
                <th>Вопросов</th>
                <th>Действия</th>
              </tr>
            </thead>
            <tbody>
              {displayCategories.map((category) => {
                const existingLangs = getExistingTranslations(category);
                const missingLangs = getMissingLanguages(category);

                return (
                  <tr key={category.id}>
                    <td style={{ fontSize: '1.5rem' }}>{category.emoji}</td>
                    <td>{category.name}</td>
                    <td>
                      <div style={{ display: 'flex', gap: '4px', flexWrap: 'wrap', alignItems: 'center' }}>
                        {/* Show existing translations */}
                        {SUPPORTED_LANGUAGES.filter(l => existingLangs.includes(l.code)).map(l => (
                          <span
                            key={l.code}
                            title={`${l.name} - есть`}
                            style={{
                              fontSize: '16px',
                              opacity: l.code === category.languageCode ? 1 : 0.6,
                              cursor: 'default'
                            }}
                          >
                            {l.flag}
                          </span>
                        ))}
                        {/* Add translation dropdown */}
                        {missingLangs.length > 0 && (
                          <div style={{ position: 'relative', display: 'inline-block' }}>
                            <select
                              style={{
                                fontSize: '12px',
                                padding: '2px 4px',
                                borderRadius: '4px',
                                border: '1px solid #ddd',
                                cursor: 'pointer',
                                backgroundColor: '#f0f0f0'
                              }}
                              value=""
                              onChange={(e) => {
                                if (e.target.value) {
                                  openTranslationModal(category, e.target.value);
                                }
                              }}
                            >
                              <option value="">+ перевод</option>
                              {missingLangs.map(l => (
                                <option key={l.code} value={l.code}>
                                  {l.flag} {l.name}
                                </option>
                              ))}
                            </select>
                          </div>
                        )}
                      </div>
                    </td>
                    <td>{category.description || '—'}</td>
                    <td>
                      <span className="badge">{category.questionsCount || 0}</span>
                    </td>
                    <td>
                      <div className="actions">
                        <button
                          className="btn btn-sm btn-secondary"
                          onClick={() => openModal(category)}
                        >
                          ✏️
                        </button>
                        <button
                          className="btn btn-sm btn-danger"
                          onClick={() => {
                            if (confirm('Удалить категорию?')) {
                              deleteMutation.mutate(category.id);
                            }
                          }}
                        >
                          🗑️
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        ) : (
          <div className="empty-state">
            <p>Категорий пока нет</p>
            <button className="btn btn-primary" onClick={() => openModal()}>
              ➕ Создать первую категорию
            </button>
          </div>
        )}
      </div>

      {isModalOpen && (
        <div className="modal-overlay" onClick={closeModal}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h3>
              {editingCategory
                ? `Редактировать категорию (${SUPPORTED_LANGUAGES.find(l => l.code === form.languageCode)?.flag} ${SUPPORTED_LANGUAGES.find(l => l.code === form.languageCode)?.name})`
                : translatingFrom
                  ? `Добавить перевод (${SUPPORTED_LANGUAGES.find(l => l.code === form.languageCode)?.flag} ${SUPPORTED_LANGUAGES.find(l => l.code === form.languageCode)?.name})`
                  : 'Новая категория'
              }
            </h3>

            {/* Show existing translations indicator */}
            {translationGroup.length > 0 && (
              <div style={{
                display: 'flex',
                gap: '8px',
                marginBottom: '16px',
                padding: '8px 12px',
                backgroundColor: '#f0f7ff',
                borderRadius: '8px',
                alignItems: 'center',
                flexWrap: 'wrap'
              }}>
                <span style={{ fontSize: '12px', color: '#666' }}>Переводы:</span>
                {SUPPORTED_LANGUAGES.map(lang => {
                  const hasTranslation = translationGroup.some(c => c.languageCode === lang.code);
                  const isCurrentLang = form.languageCode === lang.code;
                  return (
                    <span
                      key={lang.code}
                      style={{
                        fontSize: '16px',
                        opacity: hasTranslation ? 1 : 0.3,
                        padding: '2px 6px',
                        borderRadius: '4px',
                        backgroundColor: isCurrentLang ? '#3498db' : 'transparent',
                        cursor: 'default'
                      }}
                      title={hasTranslation ? `${lang.name} - есть` : `${lang.name} - нет перевода`}
                    >
                      {lang.flag}
                    </span>
                  );
                })}
              </div>
            )}

            {/* Show original category when translating */}
            {translatingFrom && (
              <div style={{
                backgroundColor: '#f5f5f5',
                padding: '12px',
                borderRadius: '8px',
                marginBottom: '16px',
                borderLeft: '4px solid #3498db'
              }}>
                <div style={{ fontSize: '12px', color: '#666', marginBottom: '8px' }}>
                  Оригинал ({SUPPORTED_LANGUAGES.find(l => l.code === translatingFrom.languageCode)?.name}):
                </div>
                <div style={{ fontWeight: 500, marginBottom: '4px' }}>
                  {translatingFrom.emoji} {translatingFrom.name}
                </div>
                {translatingFrom.description && (
                  <div style={{ fontSize: '14px', color: '#666' }}>
                    {translatingFrom.description}
                  </div>
                )}
              </div>
            )}

            <form onSubmit={handleSubmit}>
              <div className="form-row">
                <div className="form-group">
                  <label>Язык *</label>
                  <select
                    value={form.languageCode}
                    onChange={(e) => {
                      const newLang = e.target.value;
                      // Check if we have a translation for this language
                      const existingTranslation = translationGroup.find(c => c.languageCode === newLang);

                      if (existingTranslation) {
                        // Load existing translation
                        setEditingCategory(existingTranslation);
                        setForm({
                          name: existingTranslation.name,
                          description: existingTranslation.description || '',
                          emoji: existingTranslation.emoji || '',
                          languageCode: newLang,
                          translationGroupId: existingTranslation.translationGroupId,
                        });
                      } else {
                        // No translation exists - clear fields for new translation
                        setEditingCategory(null);
                        setForm({
                          ...form,
                          languageCode: newLang,
                          name: '',
                          description: '',
                        });
                      }
                    }}
                    required
                    disabled={!!translatingFrom}
                  >
                    {SUPPORTED_LANGUAGES.map((lang) => (
                      <option key={lang.code} value={lang.code}>
                        {lang.flag} {lang.name}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label>Emoji</label>
                  <input
                    type="text"
                    value={form.emoji}
                    onChange={(e) => setForm({ ...form, emoji: e.target.value })}
                    placeholder="🎯"
                    maxLength={4}
                    disabled={!!translatingFrom} // Keep emoji same for translations
                  />
                </div>
                <div className="form-group">
                  <label>Название *</label>
                  <input
                    type="text"
                    value={form.name}
                    onChange={(e) => setForm({ ...form, name: e.target.value })}
                    placeholder={translatingFrom ? "Введите перевод названия..." : "География"}
                    required
                  />
                </div>
              </div>
              <div className="form-group">
                <label>Описание</label>
                <textarea
                  value={form.description}
                  onChange={(e) => setForm({ ...form, description: e.target.value })}
                  placeholder={translatingFrom ? "Введите перевод описания..." : "Вопросы о странах и городах"}
                />
              </div>
              <div className="modal-actions">
                <button type="button" className="btn btn-secondary" onClick={closeModal}>
                  Отмена
                </button>
                <button
                  type="submit"
                  className="btn btn-primary"
                  disabled={createMutation.isPending || updateMutation.isPending}
                >
                  {editingCategory ? 'Сохранить' : translatingFrom ? 'Добавить перевод' : 'Создать'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
