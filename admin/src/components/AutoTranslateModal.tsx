import { useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { autoTranslateQuestions } from '../services/api';

interface Props {
  isOpen: boolean;
  onClose: () => void;
}

export default function AutoTranslateModal({ isOpen, onClose }: Props) {
  const [isTranslating, setIsTranslating] = useState(false);
  const [progress, setProgress] = useState({ current: 0, total: 0 });
  const [logs, setLogs] = useState<Array<{ message: string; type: string }>>([]);
  const [result, setResult] = useState<{ translated: number; skipped: number; failed: number } | null>(null);
  const [error, setError] = useState<string | null>(null);

  const queryClient = useQueryClient();

  const handleTranslate = async () => {
    setIsTranslating(true);
    setError(null);
    setLogs([]);
    setResult(null);
    setProgress({ current: 0, total: 0 });

    try {
      await autoTranslateQuestions((event) => {
        if (event.type === 'progress') {
          setProgress({
            current: event.data.current || 0,
            total: event.data.total || 0
          });
        } else if (event.type === 'log') {
          setLogs(prev => [...prev, {
            message: event.data.message,
            type: event.data.type || 'info'
          }]);
        } else if (event.type === 'complete') {
          setResult({
            translated: event.data.translated || 0,
            skipped: event.data.skipped || 0,
            failed: event.data.failed || 0
          });
        }
      });

      queryClient.invalidateQueries({ queryKey: ['questions'] });
    } catch (err: any) {
      setError(err.message || 'Failed to translate questions');
    } finally {
      setIsTranslating(false);
    }
  };

  const handleClose = () => {
    if (!isTranslating) {
      setLogs([]);
      setResult(null);
      setProgress({ current: 0, total: 0 });
      onClose();
    }
  };

  if (!isOpen) return null;

  const progressPercent = progress.total > 0 ? Math.round((progress.current / progress.total) * 100) : 0;

  return (
    <div className="modal-overlay" onClick={handleClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()} style={{ maxWidth: '700px' }}>
        <div className="modal-header">
          <h2>🌐 Автоматический перевод вопросов</h2>
        </div>

        <div className="modal-body">
          <div style={{ marginBottom: '20px', padding: '12px', background: '#f0f8ff', borderRadius: '8px', fontSize: '0.9rem' }}>
            <strong>Как это работает:</strong>
            <ul style={{ marginTop: '8px', marginBottom: 0, paddingLeft: '20px' }}>
              <li>Находит все вопросы на русском языке</li>
              <li>Проверяет, какие переводы отсутствуют</li>
              <li>Автоматически переводит на недостающие языки через MyMemory API</li>
              <li>Добавляет переводы в базу данных</li>
            </ul>
          </div>

          {!isTranslating && !result && (
            <div style={{ marginBottom: '20px', padding: '12px', background: '#fff3cd', borderRadius: '8px', fontSize: '0.9rem' }}>
              ⚠️ Процесс может занять несколько минут в зависимости от количества вопросов
            </div>
          )}

          {isTranslating && (
            <div style={{ marginBottom: '20px' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '8px' }}>
                <span style={{ fontWeight: '500' }}>
                  Обработано: {progress.current} / {progress.total}
                </span>
                <span style={{ fontWeight: '600', color: '#3498db' }}>
                  {progressPercent}%
                </span>
              </div>
              <div style={{
                width: '100%',
                height: '24px',
                background: '#e9ecef',
                borderRadius: '12px',
                overflow: 'hidden'
              }}>
                <div style={{
                  width: `${progressPercent}%`,
                  height: '100%',
                  background: 'linear-gradient(90deg, #3498db 0%, #2ecc71 100%)',
                  transition: 'width 0.3s ease',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'flex-end',
                  paddingRight: '8px'
                }}>
                  {progressPercent > 10 && (
                    <span style={{ color: 'white', fontSize: '0.75rem', fontWeight: '600' }}>
                      {progressPercent}%
                    </span>
                  )}
                </div>
              </div>
            </div>
          )}

          {logs.length > 0 && (
            <div style={{
              marginBottom: '20px',
              maxHeight: '300px',
              overflowY: 'auto',
              padding: '12px',
              background: '#f8f9fa',
              borderRadius: '8px',
              fontSize: '0.85rem',
              fontFamily: 'monospace'
            }}>
              {logs.map((log, idx) => (
                <div
                  key={idx}
                  style={{
                    padding: '4px 0',
                    color: log.type === 'error' ? '#dc3545' : log.type === 'skip' ? '#6c757d' : '#28a745',
                    borderBottom: idx < logs.length - 1 ? '1px solid #dee2e6' : 'none'
                  }}
                >
                  {log.type === 'error' && '❌ '}
                  {log.type === 'skip' && '⏭️ '}
                  {log.type === 'translating' && '🔄 '}
                  {log.message}
                </div>
              ))}
            </div>
          )}

          {result && (
            <div style={{ marginBottom: '20px', padding: '16px', background: '#d4edda', borderRadius: '8px' }}>
              <div style={{ fontWeight: '600', marginBottom: '8px', color: '#155724' }}>
                ✅ Перевод завершён!
              </div>
              <div style={{ fontSize: '0.9rem', color: '#155724' }}>
                <div>📝 Переведено: <strong>{result.translated}</strong> вопросов</div>
                <div>⏭️ Пропущено: <strong>{result.skipped}</strong> (уже имеют переводы)</div>
                {result.failed > 0 && (
                  <div style={{ color: '#856404' }}>⚠️ Ошибок: <strong>{result.failed}</strong></div>
                )}
              </div>
            </div>
          )}

          {error && (
            <div style={{ marginBottom: '20px', padding: '12px', background: '#f8d7da', color: '#721c24', borderRadius: '8px' }}>
              ❌ {error}
            </div>
          )}

          <div style={{ display: 'flex', gap: '10px' }}>
            {!isTranslating && !result && (
              <button
                onClick={handleTranslate}
                className="btn btn-primary"
              >
                🚀 Начать перевод
              </button>
            )}
            <button
              onClick={handleClose}
              disabled={isTranslating}
              className="btn btn-secondary"
            >
              {result ? 'Закрыть' : 'Отмена'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
