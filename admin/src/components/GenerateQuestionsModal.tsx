import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { generateQuestionsStream, createQuestion, type GeneratedQuestion } from '../services/api';
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
  { code: 'en', name: 'English', flag: '🇬🇧' },
];

export default function GenerateQuestionsModal({ isOpen, onClose, categories }: Props) {
  const [count, setCount] = useState(3);
  const [selectedLanguages, setSelectedLanguages] = useState<string[]>(['ru', 'de']);
  const [selectedCategoryId, setSelectedCategoryId] = useState<number | null>(null);
  const [difficulty, setDifficulty] = useState<string>('medium');
  const [generatedQuestions, setGeneratedQuestions] = useState<GeneratedQuestion[] | null>(null);
  const [isGenerating, setIsGenerating] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [progress, setProgress] = useState(0);
  const [progressMessage, setProgressMessage] = useState('');
  const [logs, setLogs] = useState<string[]>([]);
  const [selectedQuestions, setSelectedQuestions] = useState<Set<string>>(new Set());

  const queryClient = useQueryClient();

  const saveMutation = useMutation({
    mutationFn: async (questions: GeneratedQuestion[]) => {
      // Get the selected category to find its TranslationGroupId
      const selectedCategory = selectedCategoryId
        ? categories.find(c => c.id === selectedCategoryId)
        : null;

      let success = 0;
      let duplicates = 0;
      let failed = 0;

      for (const q of questions) {
        try {
          // Find the category for this language that matches the selected category's translation group
          let categoryId = selectedCategoryId!;
          if (selectedCategory?.translationGroupId) {
            const translatedCategory = categories.find(c =>
              c.languageCode === q.languageCode &&
              c.translationGroupId === selectedCategory.translationGroupId
            );
            categoryId = translatedCategory?.id || selectedCategoryId!;
          }

          await createQuestion({
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
          success++;
        } catch (err: any) {
          // Check if error is due to duplicate question (409 Conflict)
          if (err.response?.status === 409 || err.message?.includes('Duplicate')) {
            console.log('Skipping duplicate question:', q.text);
            duplicates++;
          } else {
            console.error('Failed to add question:', q, err);
            failed++;
          }
        }
      }

      console.log(`Saved questions: ${success} success, ${duplicates} duplicates, ${failed} failed`);
      return { success, duplicates, failed };
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
    setProgress(0);
    setProgressMessage('');
    setLogs([]);

    try {
      const categoryName = selectedCategoryId
        ? categories.find(c => c.id === selectedCategoryId)?.name
        : undefined;

      console.log('Generating questions:', { count, selectedLanguages, categoryName, difficulty });

      const questions = await generateQuestionsStream(
        count,
        selectedLanguages,
        categoryName,
        difficulty,
        (event) => {
          console.log('Event received:', event);
          if (event.type === 'progress') {
            setProgress(event.data.progress || 0);
            setProgressMessage(event.data.message || '');
          } else if (event.type === 'log') {
            setLogs(prev => [...prev, event.data.message]);
          }
        }
      );

      console.log('Generated questions:', questions);
      console.log('First question:', questions[0]);
      setGeneratedQuestions(questions);

      // Select all questions by default
      const allGroupIds = new Set(questions.map(q => q.translationGroupId));
      setSelectedQuestions(allGroupIds);
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
      // Only save selected questions
      const questionsToSave = generatedQuestions.filter(q => selectedQuestions.has(q.translationGroupId));
      saveMutation.mutate(questionsToSave);
    }
  };

  const toggleQuestion = (groupId: string) => {
    setSelectedQuestions(prev => {
      const next = new Set(prev);
      if (next.has(groupId)) {
        next.delete(groupId);
      } else {
        next.add(groupId);
      }
      return next;
    });
  };

  const selectAll = () => {
    if (generatedQuestions) {
      const allGroupIds = new Set(generatedQuestions.map(q => q.translationGroupId));
      setSelectedQuestions(allGroupIds);
    }
  };

  const deselectAll = () => {
    setSelectedQuestions(new Set());
  };

  const handleClose = () => {
    setGeneratedQuestions(null);
    setError(null);
    setProgress(0);
    setProgressMessage('');
    setLogs([]);
    setSelectedQuestions(new Set());
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
                  max="50"
                  value={count}
                  onChange={(e) => setCount(parseInt(e.target.value) || 1)}
                  style={{ width: '120px', padding: '8px', border: '1px solid #ddd', borderRadius: '4px' }}
                />
                <span style={{ marginLeft: '8px', fontSize: '0.85rem', color: '#666' }}>
                  (max 50)
                </span>
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
                  Difficulty
                </label>
                <select
                  value={difficulty}
                  onChange={(e) => setDifficulty(e.target.value)}
                  style={{ width: '100%', padding: '8px', border: '1px solid #ddd', borderRadius: '4px' }}
                >
                  <option value="easy">🟢 Easy - Basic knowledge</option>
                  <option value="medium">🟡 Medium - General knowledge</option>
                  <option value="hard">🔴 Hard - Challenging</option>
                  <option value="">🎲 Mixed - All difficulties</option>
                </select>
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

              {isGenerating && (
                <div style={{ padding: '16px', background: '#f8f9fa', borderRadius: '8px', border: '1px solid #dee2e6' }}>
                  {/* Progress Bar */}
                  <div style={{ marginBottom: '12px' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '6px' }}>
                      <span style={{ fontSize: '0.9rem', fontWeight: '500' }}>
                        {progressMessage || 'Processing...'}
                      </span>
                      <span style={{ fontSize: '0.9rem', fontWeight: '600', color: '#3498db' }}>
                        {progress}%
                      </span>
                    </div>
                    <div style={{
                      width: '100%',
                      height: '24px',
                      background: '#e9ecef',
                      borderRadius: '12px',
                      overflow: 'hidden',
                      position: 'relative'
                    }}>
                      <div style={{
                        width: `${progress}%`,
                        height: '100%',
                        background: 'linear-gradient(90deg, #3498db 0%, #2ecc71 100%)',
                        transition: 'width 0.3s ease',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'flex-end',
                        paddingRight: '8px'
                      }}>
                        {progress > 10 && (
                          <span style={{ color: 'white', fontSize: '0.75rem', fontWeight: '600' }}>
                            {progress}%
                          </span>
                        )}
                      </div>
                    </div>
                  </div>

                  {/* Logs */}
                  {logs.length > 0 && (
                    <div style={{
                      marginTop: '12px',
                      padding: '12px',
                      background: '#fff',
                      borderRadius: '6px',
                      maxHeight: '200px',
                      overflowY: 'auto',
                      fontSize: '0.85rem',
                      fontFamily: 'monospace',
                      border: '1px solid #dee2e6'
                    }}>
                      <div style={{ fontWeight: '600', marginBottom: '8px', color: '#495057' }}>
                        Generated Questions:
                      </div>
                      {logs.map((log, idx) => (
                        <div
                          key={idx}
                          style={{
                            padding: '4px 0',
                            color: '#28a745',
                            borderBottom: idx < logs.length - 1 ? '1px solid #f0f0f0' : 'none'
                          }}
                        >
                          ✓ {log}
                        </div>
                      ))}
                    </div>
                  )}
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
                  disabled={isGenerating}
                  className="btn btn-secondary"
                >
                  Cancel
                </button>
              </div>
            </div>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
              {/* Select/Deselect buttons */}
              <div style={{ display: 'flex', gap: '10px', padding: '12px', background: '#f8f9fa', borderRadius: '8px' }}>
                <button onClick={selectAll} className="btn btn-secondary" style={{ fontSize: '0.9rem' }}>
                  ✓ Select All
                </button>
                <button onClick={deselectAll} className="btn btn-secondary" style={{ fontSize: '0.9rem' }}>
                  ✗ Deselect All
                </button>
                <div style={{ marginLeft: 'auto', display: 'flex', alignItems: 'center', fontWeight: '500' }}>
                  Selected: {selectedQuestions.size} / {Object.keys(groupedQuestions || {}).length}
                </div>
              </div>

              {/* Preview generated questions */}
              {Object.entries(groupedQuestions || {}).map(([groupId, questions], idx) => {
                const isSelected = selectedQuestions.has(groupId);
                return (
                  <div
                    key={groupId}
                    style={{
                      border: `2px solid ${isSelected ? '#27ae60' : '#ddd'}`,
                      borderRadius: '8px',
                      padding: '16px',
                      background: isSelected ? '#f0fff4' : 'white',
                      transition: 'all 0.2s ease',
                      cursor: 'pointer'
                    }}
                    onClick={() => toggleQuestion(groupId)}
                  >
                    <div style={{ display: 'flex', alignItems: 'center', marginBottom: '12px' }}>
                      <input
                        type="checkbox"
                        checked={isSelected}
                        onChange={() => toggleQuestion(groupId)}
                        onClick={(e) => e.stopPropagation()}
                        style={{
                          width: '20px',
                          height: '20px',
                          marginRight: '12px',
                          cursor: 'pointer'
                        }}
                      />
                      <h3 style={{ fontWeight: 'bold', fontSize: '1.1rem', margin: 0 }}>
                        Question {idx + 1}
                      </h3>
                    </div>
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
                );
              })}

              <div style={{ display: 'flex', gap: '10px' }}>
                <button
                  onClick={handleSave}
                  disabled={saveMutation.isPending || selectedQuestions.size === 0}
                  className="btn"
                  style={{ background: '#27ae60', color: 'white' }}
                >
                  {saveMutation.isPending ? 'Saving...' : `Save Selected (${selectedQuestions.size})`}
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
