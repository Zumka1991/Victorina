import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getQuestions, getCategories, createQuestion, updateQuestion, deleteQuestion } from '../services/api';
import type { Question } from '../types';

export default function Questions() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [categoryFilter, setCategoryFilter] = useState<number | undefined>();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingQuestion, setEditingQuestion] = useState<Question | null>(null);
  const [form, setForm] = useState({
    categoryId: 0,
    text: '',
    correctAnswer: '',
    wrongAnswer1: '',
    wrongAnswer2: '',
    wrongAnswer3: '',
    explanation: '',
    imageUrl: '',
  });

  const { data: questionsData, isLoading } = useQuery({
    queryKey: ['questions', page, categoryFilter],
    queryFn: () => getQuestions(page, 20, categoryFilter),
  });

  const { data: categories } = useQuery({
    queryKey: ['categories'],
    queryFn: getCategories,
  });

  const createMutation = useMutation({
    mutationFn: createQuestion,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['questions'] });
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      closeModal();
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Omit<Question, 'id'> }) =>
      updateQuestion(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['questions'] });
      closeModal();
    },
  });

  const deleteMutation = useMutation({
    mutationFn: deleteQuestion,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['questions'] });
      queryClient.invalidateQueries({ queryKey: ['categories'] });
    },
  });

  const openModal = (question?: Question) => {
    if (question) {
      setEditingQuestion(question);
      setForm({
        categoryId: question.categoryId,
        text: question.text,
        correctAnswer: question.correctAnswer,
        wrongAnswer1: question.wrongAnswer1,
        wrongAnswer2: question.wrongAnswer2,
        wrongAnswer3: question.wrongAnswer3,
        explanation: question.explanation || '',
        imageUrl: question.imageUrl || '',
      });
    } else {
      setEditingQuestion(null);
      setForm({
        categoryId: categories?.[0]?.id || 0,
        text: '',
        correctAnswer: '',
        wrongAnswer1: '',
        wrongAnswer2: '',
        wrongAnswer3: '',
        explanation: '',
        imageUrl: '',
      });
    }
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setEditingQuestion(null);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (editingQuestion) {
      updateMutation.mutate({ id: editingQuestion.id, data: form });
    } else {
      createMutation.mutate(form);
    }
  };

  const totalPages = questionsData ? Math.ceil(questionsData.total / 20) : 0;

  if (isLoading) {
    return <div>Загрузка...</div>;
  }

  return (
    <div>
      <div className="page-header">
        <h2>❓ Вопросы</h2>
        <button className="btn btn-primary" onClick={() => openModal()}>
          ➕ Добавить вопрос
        </button>
      </div>

      <div className="filter-bar">
        <label>Фильтр по категории:</label>
        <select
          value={categoryFilter || ''}
          onChange={(e) => {
            setCategoryFilter(e.target.value ? Number(e.target.value) : undefined);
            setPage(1);
          }}
        >
          <option value="">Все категории</option>
          {categories?.map((cat) => (
            <option key={cat.id} value={cat.id}>
              {cat.emoji} {cat.name}
            </option>
          ))}
        </select>
        <span style={{ marginLeft: 'auto', color: '#666' }}>
          Всего: {questionsData?.total || 0} вопросов
        </span>
      </div>

      <div className="card">
        {questionsData && questionsData.items.length > 0 ? (
          <>
            <table>
              <thead>
                <tr>
                  <th style={{ width: '40%' }}>Вопрос</th>
                  <th>Категория</th>
                  <th>Правильный ответ</th>
                  <th>Картинка</th>
                  <th>Действия</th>
                </tr>
              </thead>
              <tbody>
                {questionsData.items.map((question) => (
                  <tr key={question.id}>
                    <td>{question.text}</td>
                    <td>
                      <span className="badge">{question.category}</span>
                    </td>
                    <td style={{ color: '#27ae60', fontWeight: 500 }}>
                      {question.correctAnswer}
                    </td>
                    <td>
                      {question.imageUrl ? (
                        <img
                          src={question.imageUrl}
                          alt=""
                          style={{ width: '40px', height: '40px', objectFit: 'cover', borderRadius: '4px' }}
                        />
                      ) : (
                        <span style={{ color: '#999' }}>—</span>
                      )}
                    </td>
                    <td>
                      <div className="actions">
                        <button
                          className="btn btn-sm btn-secondary"
                          onClick={() => openModal(question)}
                        >
                          ✏️
                        </button>
                        <button
                          className="btn btn-sm btn-danger"
                          onClick={() => {
                            if (confirm('Удалить вопрос?')) {
                              deleteMutation.mutate(question.id);
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

            {totalPages > 1 && (
              <div className="pagination">
                <button
                  className="btn btn-sm btn-secondary"
                  disabled={page === 1}
                  onClick={() => setPage(page - 1)}
                >
                  ← Назад
                </button>
                <span>
                  Страница {page} из {totalPages}
                </span>
                <button
                  className="btn btn-sm btn-secondary"
                  disabled={page === totalPages}
                  onClick={() => setPage(page + 1)}
                >
                  Вперёд →
                </button>
              </div>
            )}
          </>
        ) : (
          <div className="empty-state">
            <p>Вопросов пока нет</p>
            <button className="btn btn-primary" onClick={() => openModal()}>
              ➕ Создать первый вопрос
            </button>
          </div>
        )}
      </div>

      {isModalOpen && (
        <div className="modal-overlay" onClick={closeModal}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h3>{editingQuestion ? 'Редактировать вопрос' : 'Новый вопрос'}</h3>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Категория *</label>
                <select
                  value={form.categoryId}
                  onChange={(e) => setForm({ ...form, categoryId: Number(e.target.value) })}
                  required
                >
                  <option value="">Выберите категорию</option>
                  {categories?.map((cat) => (
                    <option key={cat.id} value={cat.id}>
                      {cat.emoji} {cat.name}
                    </option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label>Текст вопроса *</label>
                <textarea
                  value={form.text}
                  onChange={(e) => setForm({ ...form, text: e.target.value })}
                  placeholder="Столица Франции?"
                  required
                />
              </div>

              <div className="form-group">
                <label style={{ color: '#27ae60' }}>✓ Правильный ответ *</label>
                <input
                  type="text"
                  value={form.correctAnswer}
                  onChange={(e) => setForm({ ...form, correctAnswer: e.target.value })}
                  placeholder="Париж"
                  required
                />
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label style={{ color: '#e74c3c' }}>✗ Неправильный ответ 1 *</label>
                  <input
                    type="text"
                    value={form.wrongAnswer1}
                    onChange={(e) => setForm({ ...form, wrongAnswer1: e.target.value })}
                    placeholder="Лондон"
                    required
                  />
                </div>
                <div className="form-group">
                  <label style={{ color: '#e74c3c' }}>✗ Неправильный ответ 2 *</label>
                  <input
                    type="text"
                    value={form.wrongAnswer2}
                    onChange={(e) => setForm({ ...form, wrongAnswer2: e.target.value })}
                    placeholder="Берлин"
                    required
                  />
                </div>
              </div>

              <div className="form-group">
                <label style={{ color: '#e74c3c' }}>✗ Неправильный ответ 3 *</label>
                <input
                  type="text"
                  value={form.wrongAnswer3}
                  onChange={(e) => setForm({ ...form, wrongAnswer3: e.target.value })}
                  placeholder="Мадрид"
                  required
                />
              </div>

              <div className="form-group">
                <label>Пояснение (необязательно)</label>
                <textarea
                  value={form.explanation}
                  onChange={(e) => setForm({ ...form, explanation: e.target.value })}
                  placeholder="Париж — столица Франции с 987 года"
                />
              </div>

              <div className="form-group">
                <label>🖼️ URL картинки (необязательно)</label>
                <input
                  type="url"
                  value={form.imageUrl}
                  onChange={(e) => setForm({ ...form, imageUrl: e.target.value })}
                  placeholder="https://example.com/image.jpg"
                />
                {form.imageUrl && (
                  <div style={{ marginTop: '8px' }}>
                    <img
                      src={form.imageUrl}
                      alt="Preview"
                      style={{ maxWidth: '200px', maxHeight: '150px', borderRadius: '8px' }}
                      onError={(e) => (e.currentTarget.style.display = 'none')}
                    />
                  </div>
                )}
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
                  {editingQuestion ? 'Сохранить' : 'Создать'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
