import { useState, useRef } from 'react';
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query';
import { exportBackup, importBackup, getStats } from '../services/api';
import PasswordConfirmModal from '../components/PasswordConfirmModal';

export default function Backup() {
  const queryClient = useQueryClient();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [showImportModal, setShowImportModal] = useState(false);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);

  const { data: stats } = useQuery({
    queryKey: ['stats'],
    queryFn: getStats,
  });

  const exportMutation = useMutation({
    mutationFn: exportBackup,
    onSuccess: (data) => {
      // Create downloadable JSON file
      const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `victorina-backup-${new Date().toISOString().split('T')[0]}.json`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);

      setMessage({
        type: 'success',
        text: `Бэкап создан! Категорий: ${data.categories.length}, Вопросов: ${data.questions.length}`
      });
      setTimeout(() => setMessage(null), 5000);
    },
    onError: () => {
      setMessage({ type: 'error', text: 'Ошибка при создании бэкапа' });
      setTimeout(() => setMessage(null), 5000);
    },
  });

  const importMutation = useMutation({
    mutationFn: importBackup,
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['questions'] });
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      queryClient.invalidateQueries({ queryKey: ['stats'] });
      setMessage({
        type: 'success',
        text: `${data.message} Восстановлено категорий: ${data.categoriesCount}, вопросов: ${data.questionsCount}`
      });
      setSelectedFile(null);
      if (fileInputRef.current) fileInputRef.current.value = '';
      setTimeout(() => setMessage(null), 5000);
    },
    onError: (error: any) => {
      const errorMsg = error.response?.data?.error || 'Ошибка при восстановлении бэкапа';
      setMessage({ type: 'error', text: errorMsg });
      setTimeout(() => setMessage(null), 5000);
    },
  });

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setSelectedFile(file);
    }
  };

  const handleImportConfirm = async () => {
    if (!selectedFile) return;

    try {
      const text = await selectedFile.text();
      const data = JSON.parse(text);
      importMutation.mutate(data);
    } catch (error) {
      setMessage({ type: 'error', text: 'Неверный формат файла бэкапа' });
      setTimeout(() => setMessage(null), 5000);
    }
  };

  return (
    <div>
      <div className="page-header">
        <h2>💾 Бэкап и восстановление</h2>
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

      {/* Current Database Stats */}
      <div className="card">
        <h3 style={{ marginBottom: '15px' }}>📊 Текущая база данных</h3>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '15px' }}>
          <div style={{ padding: '15px', background: '#f8f9fa', borderRadius: '8px' }}>
            <div style={{ fontSize: '0.9rem', color: '#666', marginBottom: '5px' }}>Категорий</div>
            <div style={{ fontSize: '1.5rem', fontWeight: 'bold', color: '#3498db' }}>
              {stats?.totalCategories || 0}
            </div>
          </div>
          <div style={{ padding: '15px', background: '#f8f9fa', borderRadius: '8px' }}>
            <div style={{ fontSize: '0.9rem', color: '#666', marginBottom: '5px' }}>Вопросов</div>
            <div style={{ fontSize: '1.5rem', fontWeight: 'bold', color: '#27ae60' }}>
              {stats?.totalQuestions || 0}
            </div>
          </div>
          <div style={{ padding: '15px', background: '#f8f9fa', borderRadius: '8px' }}>
            <div style={{ fontSize: '0.9rem', color: '#666', marginBottom: '5px' }}>Пользователей</div>
            <div style={{ fontSize: '1.5rem', fontWeight: 'bold', color: '#9b59b6' }}>
              {stats?.totalUsers || 0}
            </div>
          </div>
        </div>
      </div>

      {/* Export Backup */}
      <div className="card" style={{ borderColor: '#27ae60' }}>
        <h3 style={{ marginBottom: '15px', color: '#27ae60' }}>📥 Экспорт бэкапа</h3>
        <p style={{ color: '#666', lineHeight: 1.6, marginBottom: '15px' }}>
          Создайте резервную копию всех категорий и вопросов в формате JSON.
          Файл будет сохранен на ваш компьютер.
        </p>
        <p style={{ color: '#666', lineHeight: 1.6, marginBottom: '20px' }}>
          <strong>Что сохраняется:</strong> Все категории, вопросы с ответами, переводы и связи между ними.
          <br />
          <strong>Что НЕ сохраняется:</strong> Данные пользователей, статистика игр.
        </p>
        <button
          onClick={() => exportMutation.mutate()}
          disabled={exportMutation.isPending}
          className="btn"
          style={{
            backgroundColor: '#27ae60',
            color: 'white',
            padding: '12px 24px',
            fontSize: '1rem',
            fontWeight: 'bold',
          }}
        >
          {exportMutation.isPending ? '⏳ Создание бэкапа...' : '💾 Скачать бэкап'}
        </button>
      </div>

      {/* Import Backup */}
      <div className="card" style={{ borderColor: '#e74c3c' }}>
        <h3 style={{ marginBottom: '15px', color: '#e74c3c' }}>📤 Восстановление из бэкапа</h3>
        <p style={{ color: '#666', lineHeight: 1.6, marginBottom: '15px' }}>
          <strong style={{ color: '#e74c3c' }}>⚠️ ВНИМАНИЕ!</strong> Восстановление из бэкапа
          полностью удалит все текущие категории и вопросы и заменит их данными из файла.
        </p>
        <p style={{ color: '#666', lineHeight: 1.6, marginBottom: '20px' }}>
          Это действие <strong>необратимо</strong>! Убедитесь, что у вас есть актуальная
          резервная копия текущих данных перед восстановлением.
        </p>

        <div style={{ marginBottom: '15px' }}>
          <label
            htmlFor="backup-file"
            className="btn btn-secondary"
            style={{
              display: 'inline-block',
              padding: '12px 24px',
              cursor: 'pointer',
            }}
          >
            📁 Выбрать файл бэкапа
          </label>
          <input
            id="backup-file"
            ref={fileInputRef}
            type="file"
            accept=".json"
            onChange={handleFileSelect}
            style={{ display: 'none' }}
          />
          {selectedFile && (
            <div style={{ marginTop: '10px', color: '#666' }}>
              Выбран файл: <strong>{selectedFile.name}</strong>
            </div>
          )}
        </div>

        <button
          onClick={() => setShowImportModal(true)}
          disabled={!selectedFile || importMutation.isPending}
          className="btn"
          style={{
            backgroundColor: '#e74c3c',
            color: 'white',
            padding: '12px 24px',
            fontSize: '1rem',
            fontWeight: 'bold',
            opacity: !selectedFile ? 0.5 : 1,
            cursor: !selectedFile ? 'not-allowed' : 'pointer',
          }}
        >
          {importMutation.isPending ? '⏳ Восстановление...' : '🔄 Восстановить из бэкапа'}
        </button>
      </div>

      <PasswordConfirmModal
        isOpen={showImportModal}
        onClose={() => setShowImportModal(false)}
        onConfirm={handleImportConfirm}
        title="Восстановить из бэкапа"
        message="ВНИМАНИЕ! Это действие удалит ВСЕ существующие категории и вопросы и заменит их данными из файла бэкапа. Это действие необратимо! Убедитесь, что вы выбрали правильный файл."
        confirmButtonText="Восстановить"
      />
    </div>
  );
}
