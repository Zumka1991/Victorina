import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getSettings, updateSetting } from '../services/api';

export default function Settings() {
  const queryClient = useQueryClient();
  const [message, setMessage] = useState<string | null>(null);

  const { data: settings, isLoading } = useQuery({
    queryKey: ['settings'],
    queryFn: getSettings,
  });

  const updateMutation = useMutation({
    mutationFn: ({ key, value }: { key: string; value: string }) =>
      updateSetting(key, value),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['settings'] });
      setMessage('Настройки сохранены!');
      setTimeout(() => setMessage(null), 3000);
    },
  });

  const handleChange = (key: string, value: string) => {
    updateMutation.mutate({ key, value });
  };

  if (isLoading) {
    return <div>Загрузка...</div>;
  }

  const questionTime = settings?.find((s) => s.key === 'QuestionTimeSeconds');
  const questionsCount = settings?.find((s) => s.key === 'QuestionsPerGame');

  return (
    <div>
      <div className="page-header">
        <h2>⚙️ Настройки игры</h2>
      </div>

      {message && <div className="alert alert-success">{message}</div>}

      <div className="card">
        <h3 style={{ marginBottom: '20px' }}>Параметры игры</h3>

        <div className="form-group">
          <label>⏱️ Время на ответ (секунды)</label>
          <select
            value={questionTime?.value || '15'}
            onChange={(e) => handleChange('QuestionTimeSeconds', e.target.value)}
            disabled={updateMutation.isPending}
          >
            <option value="10">10 секунд</option>
            <option value="15">15 секунд</option>
            <option value="20">20 секунд</option>
            <option value="30">30 секунд</option>
            <option value="45">45 секунд</option>
            <option value="60">60 секунд</option>
          </select>
          <p style={{ color: '#666', fontSize: '0.9rem', marginTop: '5px' }}>
            {questionTime?.description}
          </p>
        </div>

        <div className="form-group">
          <label>❓ Количество вопросов в игре</label>
          <select
            value={questionsCount?.value || '10'}
            onChange={(e) => handleChange('QuestionsPerGame', e.target.value)}
            disabled={updateMutation.isPending}
          >
            <option value="5">5 вопросов</option>
            <option value="10">10 вопросов</option>
            <option value="15">15 вопросов</option>
            <option value="20">20 вопросов</option>
          </select>
          <p style={{ color: '#666', fontSize: '0.9rem', marginTop: '5px' }}>
            {questionsCount?.description}
          </p>
        </div>
      </div>

      <div className="card">
        <h3 style={{ marginBottom: '20px' }}>ℹ️ Информация</h3>
        <p style={{ color: '#666', lineHeight: 1.6 }}>
          Эти настройки применяются ко всем новым играм. Уже начатые игры
          используют настройки, которые были актуальны на момент их создания.
        </p>
        <p style={{ color: '#666', lineHeight: 1.6, marginTop: '10px' }}>
          <strong>Таймаут:</strong> Если игрок не ответит на вопрос в течение указанного времени,
          ответ автоматически засчитается как неправильный.
        </p>
      </div>
    </div>
  );
}
