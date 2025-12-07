import { useState } from 'react';

interface Props {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title: string;
  message: string;
  confirmButtonText?: string;
}

const CORRECT_PASSWORD = '13781001gg';

export default function PasswordConfirmModal({
  isOpen,
  onClose,
  onConfirm,
  title,
  message,
  confirmButtonText = 'Подтвердить'
}: Props) {
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  if (!isOpen) return null;

  const handleConfirm = () => {
    if (password === CORRECT_PASSWORD) {
      onConfirm();
      handleClose();
    } else {
      setError('Неверный пароль!');
    }
  };

  const handleClose = () => {
    setPassword('');
    setError('');
    onClose();
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleConfirm();
    }
  };

  return (
    <div className="modal-overlay" onClick={handleClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{title}</h2>
        </div>

        <div className="modal-body">
          <p style={{ marginBottom: '20px', color: '#666', lineHeight: 1.6 }}>
            {message}
          </p>

          <div style={{ marginBottom: '20px' }}>
            <label style={{ display: 'block', marginBottom: '8px', fontWeight: '500' }}>
              Введите пароль для подтверждения:
            </label>
            <input
              type="password"
              value={password}
              onChange={(e) => {
                setPassword(e.target.value);
                setError('');
              }}
              onKeyDown={handleKeyDown}
              placeholder="Введите пароль"
              autoFocus
              style={{
                width: '100%',
                padding: '12px',
                border: error ? '2px solid #e74c3c' : '1px solid #ddd',
                borderRadius: '4px',
                fontSize: '1rem'
              }}
            />
            {error && (
              <div style={{ color: '#e74c3c', marginTop: '8px', fontSize: '0.9rem' }}>
                {error}
              </div>
            )}
          </div>

          <div style={{ display: 'flex', gap: '10px', justifyContent: 'flex-end' }}>
            <button
              onClick={handleClose}
              className="btn btn-secondary"
            >
              Отмена
            </button>
            <button
              onClick={handleConfirm}
              className="btn"
              style={{ background: '#e74c3c', color: 'white' }}
            >
              {confirmButtonText}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
