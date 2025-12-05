import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { generateQuestions, createQuestion, type GeneratedQuestion } from '../services/api';
import type { Category } from '../types';

interface Props {
  isOpen: boolean;
  onClose: () => void;
  categories: Category[];
}

const LANGUAGES = [
  { code: 'ru', name: 'Русский', flag: '🇷🇺' },
  { code: 'hi', name: 'हिन्दी', flag: '🇮🇳' },
  { code: 'pt', name: 'Português', flag: '🇵🇹' },
  { code: 'fa', name: 'فارسی', flag: '🇮🇷' },
  { code: 'de', name: 'Deutsch', flag: '🇩🇪' },
  { code: 'uz', name: 'Oʻzbekcha', flag: '🇺🇿' },
];

export default function GenerateQuestionsModal({ isOpen, onClose, categories }: Props) {
  const [count, setCount] = useState(3);
  const [selectedLanguages, setSelectedLanguages] = useState<string[]>(['ru', 'de']);
  const [selectedCategoryId, setSelectedCategoryId] = useState<number | null>(null);
  const [generatedQuestions, setGeneratedQuestions] = useState<GeneratedQuestion[] | null>(null);
  const [isGenerating, setIsGenerating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const queryClient = useQueryClient();

  const saveMutation = useMutation({
    mutationFn: async (questions: GeneratedQuestion[]) => {
      // Get the selected category to find its TranslationGroupId
      const selectedCategory = selectedCategoryId
        ? categories.find(c => c.id === selectedCategoryId)
        : null;

      const promises = questions.map(q => {
        // Find the category for this language that matches the selected category's translation group
        let categoryId = selectedCategoryId!;
        if (selectedCategory?.translationGroupId) {
          const translatedCategory = categories.find(c =>
            c.languageCode === q.languageCode &&
            c.translationGroupId === selectedCategory.translationGroupId
          );
          categoryId = translatedCategory?.id || selectedCategoryId!;
        }

        return createQuestion({
          text: q.text,
          correctAnswer: q.correctAnswer,
          wrongAnswer1: q.wrongAnswer1,
          wrongAnswer2: q.wrongAnswer2,
          wrongAnswer3: q.wrongAnswer3,
          explanation: q.explanation || undefined,
          categoryId: categoryId,
          languageCode: q.languageCode,
          translationGroupId: q.translationGroupId,
          imageUrl: undefined,
        });
      });
      return Promise.all(promises);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['questions'] });
      setGeneratedQuestions(null);
      onClose();
    },
  });

  const handleGenerate = async () => {
    if (selectedLanguages.length === 0) {
      setError('Please select at least one language');
      return;
    }

    setIsGenerating(true);
    setError(null);

    try {
      const categoryName = selectedCategoryId
        ? categories.find(c => c.id === selectedCategoryId)?.name
        : undefined;

      console.log('Generating questions:', { count, selectedLanguages, categoryName });
      const questions = await generateQuestions(count, selectedLanguages, categoryName);
      console.log('Generated questions:', questions);
      setGeneratedQuestions(questions);
    } catch (err: any) {
      console.error('Generation error:', err);
      const errorMsg = err.response?.data?.error || err.response?.data?.details || err.message || 'Failed to generate questions';
      setError(errorMsg);
    } finally {
      setIsGenerating(false);
    }
  };

  const handleSave = () => {
    if (generatedQuestions) {
      saveMutation.mutate(generatedQuestions);
    }
  };

  const handleClose = () => {
    setGeneratedQuestions(null);
    setError(null);
    onClose();
  };

  const toggleLanguage = (code: string) => {
    setSelectedLanguages(prev =>
      prev.includes(code)
        ? prev.filter(c => c !== code)
        : [...prev, code]
    );
  };

  console.log('GenerateQuestionsModal render, isOpen:', isOpen);

  if (!isOpen) return null;

  // Group questions by translation group
  const groupedQuestions = generatedQuestions?.reduce((acc, q) => {
    if (!acc[q.translationGroupId]) {
      acc[q.translationGroupId] = [];
    }
    acc[q.translationGroupId].push(q);
    return acc;
  }, {} as Record<string, GeneratedQuestion[]>);

  return (
    <div className="modal-overlay" onClick={handleClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Generate Questions with AI</h2>
        </div>

        <div className="modal-body">
          {!generatedQuestions ? (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
              {/* Configuration */}
              <div>
                <label style={{ display: 'block', marginBottom: '8px', fontWeight: '500' }}>
                  Number of questions
                </label>
                <input
                  type="number"
                  min="1"
                  max="10"
                  value={count}
                  onChange={(e) => setCount(parseInt(e.target.value) || 1)}
                  style={{ width: '120px', padding: '8px', border: '1px solid #ddd', borderRadius: '4px' }}
                />
              </div>

              <div>
                <label style={{ display: 'block', marginBottom: '8px', fontWeight: '500' }}>
                  Languages
                </label>
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px' }}>
                  {LANGUAGES.map(lang => (
                    <button
                      key={lang.code}
                      onClick={() => toggleLanguage(lang.code)}
                      className="btn"
                      style={{
                        background: selectedLanguages.includes(lang.code) ? '#3498db' : '#ecf0f1',
                        color: selectedLanguages.includes(lang.code) ? 'white' : '#2c3e50',
                        border: 'none'
                      }}
                    >
                      {lang.flag} {lang.name}
                    </button>
                  ))}
                </div>
              </div>

              <div>
                <label style={{ display: 'block', marginBottom: '8px', fontWeight: '500' }}>
                  Category (optional)
                </label>
                <select
                  value={selectedCategoryId || ''}
                  onChange={(e) => setSelectedCategoryId(e.target.value ? parseInt(e.target.value) : null)}
                  style={{ width: '100%', padding: '8px', border: '1px solid #ddd', borderRadius: '4px' }}
                >
                  <option value="">Any category</option>
                  {categories
                    .filter(c => c.languageCode === 'ru')
                    .map(cat => (
                      <option key={cat.id} value={cat.id}>
                        {cat.name}
                      </option>
                    ))}
                </select>
              </div>

              {error && (
                <div style={{ padding: '12px', background: '#fee', color: '#c00', borderRadius: '4px' }}>
                  {error}
                </div>
              )}

              <div style={{ display: 'flex', gap: '10px' }}>
                <button
                  onClick={handleGenerate}
                  disabled={isGenerating}
                  className="btn btn-primary"
                >
                  {isGenerating ? 'Generating...' : 'Generate'}
                </button>
                <button
                  onClick={handleClose}
                  className="btn btn-secondary"
                >
                  Cancel
                </button>
              </div>
            </div>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
              {/* Preview generated questions */}
              {Object.entries(groupedQuestions || {}).map(([groupId, questions], idx) => (
                <div key={groupId} style={{ border: '1px solid #ddd', borderRadius: '8px', padding: '16px' }}>
                  <h3 style={{ fontWeight: 'bold', fontSize: '1.1rem', marginBottom: '12px' }}>
                    Question {idx + 1}
                  </h3>
                  {questions.map(q => {
                    const lang = LANGUAGES.find(l => l.code === q.languageCode);
                    return (
                      <div key={`${groupId}-${q.languageCode}`} style={{ marginBottom: '16px' }}>
                        <div style={{ fontWeight: '500', fontSize: '0.9rem', color: '#666', marginBottom: '4px' }}>
                          {lang?.flag} {lang?.name}
                        </div>
                        <div style={{ paddingLeft: '16px', borderLeft: '3px solid #3498db' }}>
                          <div style={{ fontWeight: '500', marginBottom: '8px' }}>{q.text}</div>
                          <div style={{ fontSize: '0.9rem' }}>
                            <div style={{ color: '#27ae60' }}>✓ {q.correctAnswer}</div>
                            <div style={{ color: '#e74c3c' }}>✗ {q.wrongAnswer1}</div>
                            <div style={{ color: '#e74c3c' }}>✗ {q.wrongAnswer2}</div>
                            <div style={{ color: '#e74c3c' }}>✗ {q.wrongAnswer3}</div>
                          </div>
                          {q.explanation && (
                            <div style={{ marginTop: '8px', fontSize: '0.9rem', color: '#666', fontStyle: 'italic' }}>
                              💡 {q.explanation}
                            </div>
                          )}
                        </div>
                      </div>
                    );
                  })}
                </div>
              ))}

              <div style={{ display: 'flex', gap: '10px' }}>
                <button
                  onClick={handleSave}
                  disabled={saveMutation.isPending}
                  className="btn"
                  style={{ background: '#27ae60', color: 'white' }}
                >
                  {saveMutation.isPending ? 'Saving...' : 'Save All Questions'}
                </button>
                <button
                  onClick={() => setGeneratedQuestions(null)}
                  className="btn btn-secondary"
                >
                  Back
                </button>
                <button
                  onClick={handleClose}
                  className="btn btn-secondary"
                >
                  Cancel
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
