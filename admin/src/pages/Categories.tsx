import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getCategories, createCategory, updateCategory, deleteCategory } from '../services/api';
import type { Category } from '../types';

export default function Categories() {
  const queryClient = useQueryClient();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingCategory, setEditingCategory] = useState<Category | null>(null);
  const [form, setForm] = useState({ name: '', description: '', emoji: '' });

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

  const openModal = (category?: Category) => {
    if (category) {
      setEditingCategory(category);
      setForm({
        name: category.name,
        description: category.description || '',
        emoji: category.emoji || '',
      });
    } else {
      setEditingCategory(null);
      setForm({ name: '', description: '', emoji: '' });
    }
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setEditingCategory(null);
    setForm({ name: '', description: '', emoji: '' });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (editingCategory) {
      updateMutation.mutate({ id: editingCategory.id, data: form });
    } else {
      createMutation.mutate(form);
    }
  };

  if (isLoading) {
    return <div>Загрузка...</div>;
  }

  return (
    <div>
      <div className="page-header">
        <h2>📁 Категории</h2>
        <button className="btn btn-primary" onClick={() => openModal()}>
          ➕ Добавить категорию
        </button>
      </div>

      <div className="card">
        {categories && categories.length > 0 ? (
          <table>
            <thead>
              <tr>
                <th>Emoji</th>
                <th>Название</th>
                <th>Описание</th>
                <th>Вопросов</th>
                <th>Действия</th>
              </tr>
            </thead>
            <tbody>
              {categories.map((category) => (
                <tr key={category.id}>
                  <td style={{ fontSize: '1.5rem' }}>{category.emoji}</td>
                  <td>{category.name}</td>
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
              ))}
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
            <h3>{editingCategory ? 'Редактировать категорию' : 'Новая категория'}</h3>
            <form onSubmit={handleSubmit}>
              <div className="form-row">
                <div className="form-group">
                  <label>Emoji</label>
                  <input
                    type="text"
                    value={form.emoji}
                    onChange={(e) => setForm({ ...form, emoji: e.target.value })}
                    placeholder="🎯"
                    maxLength={4}
                  />
                </div>
                <div className="form-group">
                  <label>Название *</label>
                  <input
                    type="text"
                    value={form.name}
                    onChange={(e) => setForm({ ...form, name: e.target.value })}
                    placeholder="География"
                    required
                  />
                </div>
              </div>
              <div className="form-group">
                <label>Описание</label>
                <textarea
                  value={form.description}
                  onChange={(e) => setForm({ ...form, description: e.target.value })}
                  placeholder="Вопросы о странах и городах"
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
                  {editingCategory ? 'Сохранить' : 'Создать'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
