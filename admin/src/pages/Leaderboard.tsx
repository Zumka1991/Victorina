import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { getLeaderboard } from '../services/api';

type SortType = 'wins' | 'winrate' | 'games' | 'correct';

export default function Leaderboard() {
  const [sort, setSort] = useState<SortType>('wins');
  const [page, setPage] = useState(1);
  const pageSize = 20;

  const { data, isLoading } = useQuery({
    queryKey: ['leaderboard', sort, page],
    queryFn: () => getLeaderboard(sort, page, pageSize),
  });

  const getMedal = (index: number) => {
    if (page > 1) return index + 1 + (page - 1) * pageSize;
    if (index === 0) return '🥇';
    if (index === 1) return '🥈';
    if (index === 2) return '🥉';
    return index + 1;
  };

  const getDisplayName = (player: { username?: string; firstName?: string; lastName?: string }) => {
    if (player.username) return `@${player.username}`;
    const parts = [player.firstName, player.lastName].filter(Boolean);
    return parts.length > 0 ? parts.join(' ') : 'Игрок';
  };

  if (isLoading) {
    return <div>Загрузка...</div>;
  }

  const totalPages = Math.ceil((data?.total || 0) / pageSize);

  return (
    <div>
      <div className="page-header">
        <h2>🏆 Таблица лидеров</h2>
      </div>

      <div className="card">
        <div style={{ marginBottom: '20px', display: 'flex', gap: '10px', flexWrap: 'wrap' }}>
          <button
            className={`btn ${sort === 'wins' ? 'btn-primary' : 'btn-secondary'}`}
            onClick={() => { setSort('wins'); setPage(1); }}
          >
            По победам
          </button>
          <button
            className={`btn ${sort === 'winrate' ? 'btn-primary' : 'btn-secondary'}`}
            onClick={() => { setSort('winrate'); setPage(1); }}
          >
            По % побед
          </button>
          <button
            className={`btn ${sort === 'games' ? 'btn-primary' : 'btn-secondary'}`}
            onClick={() => { setSort('games'); setPage(1); }}
          >
            По играм
          </button>
          <button
            className={`btn ${sort === 'correct' ? 'btn-primary' : 'btn-secondary'}`}
            onClick={() => { setSort('correct'); setPage(1); }}
          >
            По правильным ответам
          </button>
        </div>

        {data && data.items.length > 0 ? (
          <>
            <table className="table">
              <thead>
                <tr>
                  <th style={{ width: '60px' }}>#</th>
                  <th>Игрок</th>
                  <th style={{ textAlign: 'center' }}>Игр</th>
                  <th style={{ textAlign: 'center' }}>Побед</th>
                  <th style={{ textAlign: 'center' }}>% побед</th>
                  <th style={{ textAlign: 'center' }}>Правильных</th>
                </tr>
              </thead>
              <tbody>
                {data.items.map((player, index) => (
                  <tr key={player.id}>
                    <td style={{ fontSize: '1.2em', textAlign: 'center' }}>
                      {getMedal(index)}
                    </td>
                    <td>
                      <strong>{getDisplayName(player)}</strong>
                    </td>
                    <td style={{ textAlign: 'center' }}>{player.gamesPlayed}</td>
                    <td style={{ textAlign: 'center' }}>{player.gamesWon}</td>
                    <td style={{ textAlign: 'center' }}>{player.winRate}%</td>
                    <td style={{ textAlign: 'center' }}>{player.totalCorrectAnswers}</td>
                  </tr>
                ))}
              </tbody>
            </table>

            {totalPages > 1 && (
              <div style={{ marginTop: '20px', display: 'flex', justifyContent: 'center', gap: '10px' }}>
                <button
                  className="btn btn-secondary"
                  disabled={page === 1}
                  onClick={() => setPage(p => p - 1)}
                >
                  ← Назад
                </button>
                <span style={{ padding: '8px 16px' }}>
                  Страница {page} из {totalPages}
                </span>
                <button
                  className="btn btn-secondary"
                  disabled={page >= totalPages}
                  onClick={() => setPage(p => p + 1)}
                >
                  Вперёд →
                </button>
              </div>
            )}
          </>
        ) : (
          <p style={{ color: '#666', textAlign: 'center', padding: '40px' }}>
            Пока нет игроков с завершёнными играми
          </p>
        )}
      </div>

      <div className="card">
        <h3>ℹ️ О рейтинге</h3>
        <p style={{ color: '#666', lineHeight: 1.6 }}>
          В таблице отображаются только игроки, которые сыграли хотя бы одну игру.
          Вы можете сортировать таблицу по разным критериям с помощью кнопок выше.
        </p>
      </div>
    </div>
  );
}
