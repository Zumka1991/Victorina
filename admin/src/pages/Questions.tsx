import { useState, useRef } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getQuestions, getCategories, createQuestion, updateQuestion, deleteQuestion, uploadImage, getFullImageUrl } from '../services/api';
import type { Question } from '../types';

export default function Questions() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [categoryFilter, setCategoryFilter] = useState<number | undefined>();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingQuestion, setEditingQuestion] = useState<Question | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);
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

  const handleFileSelect = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Validate file type
    if (!file.type.startsWith('image/')) {
      alert('Пожалуйста, выберите изображение');
      return;
    }

    // Validate file size (5MB)
    if (file.size > 5 * 1024 * 1024) {
      alert('Размер файла не должен превышать 5 МБ');
      return;
    }

    setIsUploading(true);
    try {
      const result = await uploadImage(file);
      setForm({ ...form, imageUrl: result.url });
    } catch (error) {
      console.error('Upload error:', error);
      alert('Ошибка при загрузке изображения');
    } finally {
      setIsUploading(false);
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    }
  };

  const handleRemoveImage = () => {
    setForm({ ...form, imageUrl: '' });
  };

  const totalPages = questionsData ? Math.ceil(questionsData.total / 20) : 0;

  if (isLoading) {
    return <div>Загрузка...</div>;
  }

  return (
    <div>
      <div className="page-header">
        <h2>Вопросы</h2>
        <button className="btn btn-primary" onClick={() => openModal()}>
          + Добавить вопрос
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
                          src={getFullImageUrl(question.imageUrl)}
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
                          Edit
                        </button>
                        <button
                          className="btn btn-sm btn-danger"
                          onClick={() => {
                            if (confirm('Удалить вопрос?')) {
                              deleteMutation.mutate(question.id);
                            }
                          }}
                        >
                          Del
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
                  Назад
                </button>
                <span>
                  Страница {page} из {totalPages}
                </span>
                <button
                  className="btn btn-sm btn-secondary"
                  disabled={page === totalPages}
                  onClick={() => setPage(page + 1)}
                >
                  Вперёд
                </button>
              </div>
            )}
          </>
        ) : (
          <div className="empty-state">
            <p>Вопросов пока нет</p>
            <button className="btn btn-primary" onClick={() => openModal()}>
              + Создать первый вопрос
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
                <label style={{ color: '#27ae60' }}>Правильный ответ *</label>
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
                  <label style={{ color: '#e74c3c' }}>Неправильный ответ 1 *</label>
                  <input
                    type="text"
                    value={form.wrongAnswer1}
                    onChange={(e) => setForm({ ...form, wrongAnswer1: e.target.value })}
                    placeholder="Лондон"
                    required
                  />
                </div>
                <div className="form-group">
                  <label style={{ color: '#e74c3c' }}>Неправильный ответ 2 *</label>
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
                <label style={{ color: '#e74c3c' }}>Неправильный ответ 3 *</label>
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
                <label>Картинка (необязательно)</label>
                <div style={{
                  border: '2px dashed #ddd',
                  borderRadius: '8px',
                  padding: '16px',
                  backgroundColor: '#fafafa'
                }}>
                  {form.imageUrl ? (
                    <div style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
                      <img
                        src={getFullImageUrl(form.imageUrl)}
                        alt="Preview"
                        style={{
                          maxWidth: '200px',
                          maxHeight: '150px',
                          borderRadius: '8px',
                          objectFit: 'cover',
                          border: '1px solid #ddd'
                        }}
                      />
                      <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                        <span style={{ fontSize: '12px', color: '#666', wordBreak: 'break-all' }}>
                          {form.imageUrl}
                        </span>
                        <button
                          type="button"
                          className="btn btn-sm btn-danger"
                          onClick={handleRemoveImage}
                        >
                          Удалить
                        </button>
                      </div>
                    </div>
                  ) : (
                    <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                      <input
                        ref={fileInputRef}
                        type="file"
                        accept="image/*"
                        onChange={handleFileSelect}
                        style={{ display: 'none' }}
                        id="image-upload"
                      />
                      <label
                        htmlFor="image-upload"
                        className="btn btn-secondary"
                        style={{ cursor: isUploading ? 'wait' : 'pointer' }}
                      >
                        {isUploading ? 'Загрузка...' : 'Выбрать файл'}
                      </label>
                      <span style={{ color: '#666', fontSize: '14px' }}>
                        JPG, PNG, GIF, WEBP до 5 МБ
                      </span>
                    </div>
                  )}
                </div>
              </div>

              <div className="modal-actions">
                <button type="button" className="btn btn-secondary" onClick={closeModal}>
                  Отмена
                </button>
                <button
                  type="submit"
                  className="btn btn-primary"
                  disabled={createMutation.isPending || updateMutation.isPending || isUploading}
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
