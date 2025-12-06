import { useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { createQuestion } from '../services/api';
import type { Category } from '../types';
import { SUPPORTED_LANGUAGES } from '../types';

interface Props {
  isOpen: boolean;
  onClose: () => void;
  categories: Category[];
}

export default function BulkAddQuestionsModal({ isOpen, onClose, categories }: Props) {
  const [bulkText, setBulkText] = useState('');
  const [selectedLanguage, setSelectedLanguage] = useState('ru');
  const [selectedCategoryId, setSelectedCategoryId] = useState<number | null>(null);
  const [isProcessing, setIsProcessing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [result, setResult] = useState<{ success: number; failed: number } | null>(null);

  const queryClient = useQueryClient();

  const parseQuestions = (text: string) => {
    const lines = text.split('\n').map(l => l.trim());
    const questions: Array<{
      text: string;
      answers: string[];
      correctIndex: number;
    }> = [];

    let i = 0;
    while (i < lines.length) {
      // Skip empty lines
      while (i < lines.length && !lines[i]) i++;
      if (i >= lines.length) break;

      // Line 1: Question
      const questionText = lines[i];
      if (!questionText) {
        i++;
        continue;
      }

      // Lines 2-5: Answers
      const answers: string[] = [];
      for (let j = 1; j <= 4; j++) {
        if (i + j < lines.length && lines[i + j]) {
          answers.push(lines[i + j]);
        }
      }

      if (answers.length !== 4) {
        throw new Error(`Вопрос "${questionText}": найдено ${answers.length} ответов вместо 4`);
      }

      // Line 6: Correct answer index
      const correctIndexLine = lines[i + 5];
      const correctIndex = parseInt(correctIndexLine);

      if (isNaN(correctIndex) || correctIndex < 1 || correctIndex > 4) {
        throw new Error(`Вопрос "${questionText}": неверный номер правильного ответа "${correctIndexLine}"`);
      }

      questions.push({
        text: questionText,
        answers,
        correctIndex: correctIndex - 1, // Convert to 0-based
      });

      // Move to next question (skip 2 empty lines)
      i += 8;
    }

    return questions;
  };

  const handleProcess = async () => {
    if (!selectedCategoryId) {
      setError('Выберите категорию');
      return;
    }

    if (!bulkText.trim()) {
      setError('Введите вопросы');
      return;
    }

    setIsProcessing(true);
    setError(null);
    setResult(null);

    try {
      const questions = parseQuestions(bulkText);

      let success = 0;
      let failed = 0;

      for (const q of questions) {
        try {
          await createQuestion({
            text: q.text,
            correctAnswer: q.answers[q.correctIndex],
            wrongAnswer1: q.answers[(q.correctIndex + 1) % 4],
            wrongAnswer2: q.answers[(q.correctIndex + 2) % 4],
            wrongAnswer3: q.answers[(q.correctIndex + 3) % 4],
            categoryId: selectedCategoryId,
            languageCode: selectedLanguage,
            translationGroupId: undefined,
            imageUrl: undefined,
            explanation: undefined,
          });
          success++;
        } catch (err) {
          console.error('Failed to add question:', q, err);
          failed++;
        }
      }

      setResult({ success, failed });
      queryClient.invalidateQueries({ queryKey: ['questions'] });

      if (failed === 0) {
        setTimeout(() => {
          onClose();
          setBulkText('');
          setResult(null);
        }, 2000);
      }
    } catch (err: any) {
      setError(err.message || 'Ошибка при обработке вопросов');
    } finally {
      setIsProcessing(false);
    }
  };

  const handleClose = () => {
    setBulkText('');
    setError(null);
    setResult(null);
    onClose();
  };

  if (!isOpen) return null;

  // Get categories for selected language
  const langCategories = categories.filter(c => c.languageCode === selectedLanguage);
  if (langCategories.length > 0 && !selectedCategoryId) {
    setSelectedCategoryId(langCategories[0].id);
  }

  return (
    <div className="modal-overlay" onClick={handleClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()} style={{ maxWidth: '800px' }}>
        <div className="modal-header">
          <h2>📝 Массовое добавление вопросов</h2>
        </div>

        <div className="modal-body">
          <div style={{ marginBottom: '20px', padding: '12px', background: '#f0f8ff', borderRadius: '8px', fontSize: '0.9rem' }}>
            <strong>Формат:</strong>
            <pre style={{ margin: '8px 0 0 0', fontSize: '0.85rem', lineHeight: '1.4' }}>
{`Вопрос?
Ответ 1
Ответ 2
Ответ 3
Ответ 4
1


Следующий вопрос?
...`}
            </pre>
            <div style={{ marginTop: '8px', color: '#666' }}>
              • Строка 1: Вопрос<br/>
              • Строки 2-5: Четыре варианта ответа<br/>
              • Строка 6: Номер правильного ответа (1-4)<br/>
              • Строки 7-8: Пустые строки (разделитель)<br/>
            </div>
          </div>

          <div style={{ display: 'flex', gap: '15px', marginBottom: '15px' }}>
            <div style={{ flex: 1 }}>
              <label style={{ display: 'block', marginBottom: '8px', fontWeight: '500' }}>
                Язык
              </label>
              <select
                value={selectedLanguage}
                onChange={(e) => {
                  setSelectedLanguage(e.target.value);
                  setSelectedCategoryId(null);
                }}
                style={{ width: '100%', padding: '8px', border: '1px solid #ddd', borderRadius: '8px' }}
              >
                {SUPPORTED_LANGUAGES.map(lang => (
                  <option key={lang.code} value={lang.code}>
                    {lang.flag} {lang.name}
                  </option>
                ))}
              </select>
            </div>

            <div style={{ flex: 1 }}>
              <label style={{ display: 'block', marginBottom: '8px', fontWeight: '500' }}>
                Категория
              </label>
              <select
                value={selectedCategoryId || ''}
                onChange={(e) => setSelectedCategoryId(parseInt(e.target.value))}
                style={{ width: '100%', padding: '8px', border: '1px solid #ddd', borderRadius: '8px' }}
              >
                {langCategories.map(cat => (
                  <option key={cat.id} value={cat.id}>
                    {cat.name}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div style={{ marginBottom: '15px' }}>
            <label style={{ display: 'block', marginBottom: '8px', fontWeight: '500' }}>
              Вопросы
            </label>
            <textarea
              value={bulkText}
              onChange={(e) => setBulkText(e.target.value)}
              placeholder="Вставьте вопросы в указанном формате..."
              style={{
                width: '100%',
                minHeight: '300px',
                padding: '12px',
                border: '1px solid #ddd',
                borderRadius: '8px',
                fontFamily: 'monospace',
                fontSize: '0.9rem',
                resize: 'vertical'
              }}
            />
          </div>

          {error && (
            <div style={{ padding: '12px', background: '#fee', color: '#c00', borderRadius: '4px', marginBottom: '15px' }}>
              {error}
            </div>
          )}

          {result && (
            <div style={{ padding: '12px', background: '#d4edda', color: '#155724', borderRadius: '4px', marginBottom: '15px' }}>
              ✅ Успешно добавлено: {result.success}
              {result.failed > 0 && (
                <span style={{ display: 'block', marginTop: '4px' }}>
                  ❌ Ошибок: {result.failed}
                </span>
              )}
            </div>
          )}

          <div style={{ display: 'flex', gap: '10px' }}>
            <button
              onClick={handleProcess}
              disabled={isProcessing}
              className="btn btn-primary"
            >
              {isProcessing ? 'Обработка...' : 'Добавить вопросы'}
            </button>
            <button
              onClick={handleClose}
              disabled={isProcessing}
              className="btn btn-secondary"
            >
              Закрыть
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
