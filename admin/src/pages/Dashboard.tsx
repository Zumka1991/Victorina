import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getStats, seedData, resetSeedData } from '../services/api';
import { useState } from 'react';

export default function Dashboard() {
  const queryClient = useQueryClient();
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  const { data: stats, isLoading } = useQuery({
    queryKey: ['stats'],
    queryFn: getStats,
  });

  const seedMutation = useMutation({
    mutationFn: seedData,
    onSuccess: (data) => {
      setMessage({ type: 'success', text: data.Message });
      queryClient.invalidateQueries({ queryKey: ['stats'] });
    },
    onError: () => {
      setMessage({ type: 'error', text: 'Ошибка при добавлении данных' });
    },
  });

  const resetMutation = useMutation({
    mutationFn: resetSeedData,
    onSuccess: (data) => {
      setMessage({ type: 'success', text: data.Message });
      queryClient.invalidateQueries({ queryKey: ['stats'] });
    },
    onError: () => {
      setMessage({ type: 'error', text: 'Ошибка при сбросе данных' });
    },
  });

  if (isLoading) {
    return <div>Загрузка...</div>;
  }

  return (
    <div>
      <div className="page-header">
        <h2>📊 Дашборд</h2>
      </div>

      {message && (
        <div className={`alert alert-${message.type}`}>
          {message.text}
          <button
            onClick={() => setMessage(null)}
            style={{ float: 'right', background: 'none', border: 'none', cursor: 'pointer' }}
          >
            ✕
          </button>
        </div>
      )}

      <div className="stats-grid">
        <div className="stat-card">
          <h3>{stats?.totalUsers || 0}</h3>
          <p>Пользователей</p>
        </div>
        <div className="stat-card">
          <h3>{stats?.totalGames || 0}</h3>
          <p>Всего игр</p>
        </div>
        <div className="stat-card">
          <h3>{stats?.totalQuestions || 0}</h3>
          <p>Вопросов</p>
        </div>
        <div className="stat-card">
          <h3>{stats?.totalCategories || 0}</h3>
          <p>Категорий</p>
        </div>
        <div className="stat-card">
          <h3>{stats?.gamesToday || 0}</h3>
          <p>Игр сегодня</p>
        </div>
      </div>

      <div className="card">
        <h3 style={{ marginBottom: '20px' }}>🌱 Тестовые данные</h3>
        <p style={{ marginBottom: '20px', color: '#666' }}>
          Добавьте тестовые вопросы и категории для проверки работы бота.
        </p>
        <div style={{ display: 'flex', gap: '10px' }}>
          <button
            className="btn btn-success"
            onClick={() => seedMutation.mutate()}
            disabled={seedMutation.isPending}
          >
            {seedMutation.isPending ? 'Добавление...' : '➕ Добавить тестовые данные'}
          </button>
          <button
            className="btn btn-danger"
            onClick={() => {
              if (confirm('Удалить все вопросы и категории и создать заново?')) {
                resetMutation.mutate();
              }
            }}
            disabled={resetMutation.isPending}
          >
            {resetMutation.isPending ? 'Сброс...' : '🔄 Сбросить и пересоздать'}
          </button>
        </div>
      </div>
    </div>
  );
}
