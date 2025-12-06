import { useState, useRef, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getQuestions, getCategories, createQuestion, updateQuestion, deleteQuestion, uploadImage, getFullImageUrl, getQuestionTranslations } from '../services/api';
import type { Question } from '../types';
import { SUPPORTED_LANGUAGES } from '../types';
import GenerateQuestionsModal from '../components/GenerateQuestionsModal';

export default function Questions() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [categoryFilter, setCategoryFilter] = useState<number | undefined>();
  const [languageFilter, setLanguageFilter] = useState<string | undefined>();
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [debouncedSearch, setDebouncedSearch] = useState<string>('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingQuestion, setEditingQuestion] = useState<Question | null>(null);
  const [translatingFrom, setTranslatingFrom] = useState<Question | null>(null); // Question we're adding translation for
  const [translationGroup, setTranslationGroup] = useState<Question[]>([]); // All translations for current question
  const [isUploading, setIsUploading] = useState(false);
  const [isGenerateModalOpen, setIsGenerateModalOpen] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [form, setForm] = useState({
    categoryId: 0,
    languageCode: 'ru',
    translationGroupId: undefined as string | undefined,
    text: '',
    correctAnswer: '',
    wrongAnswer1: '',
    wrongAnswer2: '',
    wrongAnswer3: '',
    explanation: '',
    imageUrl: '',
  });

  // Debounce search query to avoid excessive API calls
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(searchQuery);
      setPage(1); // Reset to first page when search changes
    }, 300); // 300ms delay

    return () => clearTimeout(timer);
  }, [searchQuery]);

  const { data: questionsData, isLoading } = useQuery({
    queryKey: ['questions', page, categoryFilter, languageFilter, debouncedSearch],
    queryFn: () => getQuestions(page, 20, categoryFilter, languageFilter, debouncedSearch || undefined),
    placeholderData: (previousData) => previousData,
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

  const openModal = async (question?: Question) => {
    if (question) {
      setEditingQuestion(question);
      setTranslatingFrom(null);
      setForm({
        categoryId: question.categoryId,
        languageCode: question.languageCode || 'ru',
        translationGroupId: question.translationGroupId,
        text: question.text,
        correctAnswer: question.correctAnswer,
        wrongAnswer1: question.wrongAnswer1,
        wrongAnswer2: question.wrongAnswer2,
        wrongAnswer3: question.wrongAnswer3,
        explanation: question.explanation || '',
        imageUrl: question.imageUrl || '',
      });
      // Load all translations for this question
      if (question.translationGroupId) {
        try {
          const translations = await getQuestionTranslations(question.translationGroupId);
          setTranslationGroup(translations);
        } catch {
          setTranslationGroup([question]);
        }
      } else {
        setTranslationGroup([question]);
      }
    } else {
      setEditingQuestion(null);
      setTranslatingFrom(null);
      setTranslationGroup([]);
      const defaultLang = languageFilter || 'ru';
      // Find first category for the default language
      const langCategories = categories?.filter(c => c.languageCode === defaultLang) || [];
      const defaultCategoryId = langCategories.length > 0 ? langCategories[0].id : (categories?.[0]?.id || 0);
      setForm({
        categoryId: defaultCategoryId,
        languageCode: defaultLang,
        translationGroupId: undefined,
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

  // Open modal to add translation for existing question
  const openTranslationModal = (question: Question, targetLang: string) => {
    setEditingQuestion(null);
    setTranslatingFrom(question);

    // Find corresponding category in target language
    const originalCategory = categories?.find(c => c.id === question.categoryId);
    // First try to find category with same TranslationGroupId
    let targetCategory = categories?.find(c =>
      c.languageCode === targetLang &&
      c.translationGroupId &&
      c.translationGroupId === originalCategory?.translationGroupId
    );
    // Fallback: find category with same emoji in target language
    if (!targetCategory) {
      targetCategory = categories?.find(c =>
        c.languageCode === targetLang && c.emoji === originalCategory?.emoji
      );
    }
    const targetCategoryId = targetCategory?.id ||
      (categories?.find(c => c.languageCode === targetLang)?.id || question.categoryId);

    setForm({
      categoryId: targetCategoryId,
      languageCode: targetLang,
      translationGroupId: question.translationGroupId,
      text: '', // Empty - user needs to translate
      correctAnswer: '',
      wrongAnswer1: '',
      wrongAnswer2: '',
      wrongAnswer3: '',
      explanation: '',
      imageUrl: question.imageUrl || '', // Keep same image
    });
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setEditingQuestion(null);
    setTranslatingFrom(null);
    setTranslationGroup([]);
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

    if (!file.type.startsWith('image/')) {
      alert('Пожалуйста, выберите изображение');
      return;
    }

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

  // Get existing translations for a question
  const getExistingTranslations = (question: Question): string[] => {
    if (!question.translationGroupId || !questionsData) return [question.languageCode];

    const translations = questionsData.items
      .filter(q => q.translationGroupId === question.translationGroupId)
      .map(q => q.languageCode);

    return [...new Set(translations)];
  };

  // Get missing languages for translation
  const getMissingLanguages = (question: Question): typeof SUPPORTED_LANGUAGES[number][] => {
    const existing = getExistingTranslations(question);
    return SUPPORTED_LANGUAGES.filter(lang => !existing.includes(lang.code));
  };

  const totalPages = questionsData ? Math.ceil(questionsData.total / 20) : 0;

  if (isLoading) {
    return <div>Загрузка...</div>;
  }

  return (
    <div>
      <div className="page-header">
        <h2>Вопросы</h2>
        <div style={{ display: 'flex', gap: '10px' }}>
          <button className="btn btn-secondary" onClick={() => {
            console.log('Generate AI button clicked');
            setIsGenerateModalOpen(true);
          }}>
            🤖 Generate with AI
          </button>
          <button className="btn btn-primary" onClick={() => openModal()}>
            + Добавить вопрос
          </button>
        </div>
      </div>

      <div className="filter-bar">
        <input
          type="text"
          placeholder="🔍 Поиск по вопросам..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          style={{
            padding: '8px 15px',
            border: '1px solid #ddd',
            borderRadius: '8px',
            minWidth: '250px',
          }}
        />
        <label>Язык:</label>
        <select
          value={languageFilter || ''}
          onChange={(e) => {
            setLanguageFilter(e.target.value || undefined);
            setPage(1);
          }}
        >
          <option value="">Все языки</option>
          {SUPPORTED_LANGUAGES.map((lang) => (
            <option key={lang.code} value={lang.code}>
              {lang.flag} {lang.name}
            </option>
          ))}
        </select>
        <label>Категория:</label>
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
                  <th style={{ width: '30%' }}>Вопрос</th>
                  <th>Переводы</th>
                  <th>Категория</th>
                  <th>Правильный ответ</th>
                  <th>Картинка</th>
                  <th>Действия</th>
                </tr>
              </thead>
              <tbody>
                {questionsData.items.map((question) => {
                  const existingLangs = getExistingTranslations(question);
                  const missingLangs = getMissingLanguages(question);

                  return (
                    <tr key={question.id}>
                      <td data-label="Вопрос">{question.text}</td>
                      <td data-label="Переводы">
                        <div style={{ display: 'flex', gap: '4px', flexWrap: 'wrap', alignItems: 'center' }}>
                          {/* Show existing translations */}
                          {SUPPORTED_LANGUAGES.filter(l => existingLangs.includes(l.code)).map(l => (
                            <span
                              key={l.code}
                              title={`${l.name} - есть`}
                              style={{
                                fontSize: '16px',
                                opacity: l.code === question.languageCode ? 1 : 0.6,
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
                                    openTranslationModal(question, e.target.value);
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
                      <td data-label="Категория">
                        <span className="badge">{question.category}</span>
                      </td>
                      <td data-label="Ответ" style={{ color: '#27ae60', fontWeight: 500 }}>
                        {question.correctAnswer}
                      </td>
                      <td data-label="Картинка">
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
                      <td data-label="Действия">
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
                  );
                })}
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
            <h3>
              {editingQuestion
                ? `Редактировать вопрос (${SUPPORTED_LANGUAGES.find(l => l.code === form.languageCode)?.flag} ${SUPPORTED_LANGUAGES.find(l => l.code === form.languageCode)?.name})`
                : translatingFrom
                  ? `Добавить перевод (${SUPPORTED_LANGUAGES.find(l => l.code === form.languageCode)?.name})`
                  : translationGroup.length > 0
                    ? `Новый перевод (${SUPPORTED_LANGUAGES.find(l => l.code === form.languageCode)?.flag} ${SUPPORTED_LANGUAGES.find(l => l.code === form.languageCode)?.name})`
                    : 'Новый вопрос'
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
                  const hasTranslation = translationGroup.some(q => q.languageCode === lang.code);
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

            {/* Show original question when translating */}
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
                <div style={{ fontWeight: 500, marginBottom: '8px' }}>{translatingFrom.text}</div>
                <div style={{ fontSize: '14px' }}>
                  <span style={{ color: '#27ae60' }}>✓ {translatingFrom.correctAnswer}</span>
                  {' • '}
                  <span style={{ color: '#999' }}>
                    {translatingFrom.wrongAnswer1}, {translatingFrom.wrongAnswer2}, {translatingFrom.wrongAnswer3}
                  </span>
                </div>
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
                      // Filter categories for new language
                      const langCategories = categories?.filter(c => c.languageCode === newLang) || [];
                      // Find the corresponding category (first by TranslationGroupId, then by emoji)
                      const currentCategory = categories?.find(c => c.id === form.categoryId);
                      let targetCategory = langCategories.find(c =>
                        c.translationGroupId && c.translationGroupId === currentCategory?.translationGroupId
                      );
                      if (!targetCategory) {
                        targetCategory = langCategories.find(c => c.emoji === currentCategory?.emoji);
                      }
                      const newCategoryId = targetCategory?.id || (langCategories.length > 0 ? langCategories[0].id : 0);

                      // Check if we have a translation for this language
                      const existingTranslation = translationGroup.find(q => q.languageCode === newLang);

                      if (existingTranslation) {
                        // Load existing translation
                        setEditingQuestion(existingTranslation);
                        setForm({
                          categoryId: existingTranslation.categoryId,
                          languageCode: newLang,
                          translationGroupId: existingTranslation.translationGroupId,
                          text: existingTranslation.text,
                          correctAnswer: existingTranslation.correctAnswer,
                          wrongAnswer1: existingTranslation.wrongAnswer1,
                          wrongAnswer2: existingTranslation.wrongAnswer2,
                          wrongAnswer3: existingTranslation.wrongAnswer3,
                          explanation: existingTranslation.explanation || '',
                          imageUrl: existingTranslation.imageUrl || '',
                        });
                      } else {
                        // No translation exists - clear fields for new translation
                        setEditingQuestion(null);
                        setForm({
                          ...form,
                          languageCode: newLang,
                          categoryId: newCategoryId,
                          text: '',
                          correctAnswer: '',
                          wrongAnswer1: '',
                          wrongAnswer2: '',
                          wrongAnswer3: '',
                          explanation: '',
                        });
                      }
                    }}
                    required
                    disabled={!!translatingFrom} // Can't change language when translating
                  >
                    {SUPPORTED_LANGUAGES.map((lang) => (
                      <option key={lang.code} value={lang.code}>
                        {lang.flag} {lang.name}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label>Категория *</label>
                  <select
                    value={form.categoryId}
                    onChange={(e) => setForm({ ...form, categoryId: Number(e.target.value) })}
                    required
                    disabled={!!translatingFrom} // Can't change category when translating
                  >
                    <option value="">Выберите категорию</option>
                    {categories
                      ?.filter((cat) => cat.languageCode === form.languageCode)
                      .map((cat) => (
                        <option key={cat.id} value={cat.id}>
                          {cat.emoji} {cat.name}
                        </option>
                      ))}
                  </select>
                </div>
              </div>

              <div className="form-group">
                <label>Текст вопроса *</label>
                <textarea
                  value={form.text}
                  onChange={(e) => setForm({ ...form, text: e.target.value })}
                  placeholder={translatingFrom ? "Введите перевод вопроса..." : "Столица Франции?"}
                  required
                />
              </div>

              <div className="form-group">
                <label style={{ color: '#27ae60' }}>Правильный ответ *</label>
                <input
                  type="text"
                  value={form.correctAnswer}
                  onChange={(e) => setForm({ ...form, correctAnswer: e.target.value })}
                  placeholder={translatingFrom ? "Перевод правильного ответа..." : "Париж"}
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
                    placeholder={translatingFrom ? "Перевод..." : "Лондон"}
                    required
                  />
                </div>
                <div className="form-group">
                  <label style={{ color: '#e74c3c' }}>Неправильный ответ 2 *</label>
                  <input
                    type="text"
                    value={form.wrongAnswer2}
                    onChange={(e) => setForm({ ...form, wrongAnswer2: e.target.value })}
                    placeholder={translatingFrom ? "Перевод..." : "Берлин"}
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
                  placeholder={translatingFrom ? "Перевод..." : "Мадрид"}
                  required
                />
              </div>

              <div className="form-group">
                <label>Пояснение (необязательно)</label>
                <textarea
                  value={form.explanation}
                  onChange={(e) => setForm({ ...form, explanation: e.target.value })}
                  placeholder={translatingFrom ? "Перевод пояснения..." : "Париж — столица Франции с 987 года"}
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
                  {editingQuestion ? 'Сохранить' : translatingFrom ? 'Добавить перевод' : 'Создать'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      <GenerateQuestionsModal
        isOpen={isGenerateModalOpen}
        onClose={() => setIsGenerateModalOpen(false)}
        categories={categories || []}
      />
    </div>
  );
}
