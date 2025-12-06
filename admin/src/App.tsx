import { useState } from 'react';
import { BrowserRouter, Routes, Route, NavLink } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import Dashboard from './pages/Dashboard';
import Categories from './pages/Categories';
import Questions from './pages/Questions';
import Leaderboard from './pages/Leaderboard';
import Settings from './pages/Settings';
import './App.css';

const queryClient = new QueryClient();

function App() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const closeMobileMenu = () => setMobileMenuOpen(false);

  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <div className="app">
          <div
            className={`mobile-overlay ${mobileMenuOpen ? 'active' : ''}`}
            onClick={closeMobileMenu}
          />

          <nav className={`sidebar ${mobileMenuOpen ? 'mobile-open' : ''}`}>
            <h1>🎯 Викторина</h1>
            <ul>
              <li>
                <NavLink
                  to="/"
                  className={({ isActive }) => isActive ? 'active' : ''}
                  onClick={closeMobileMenu}
                >
                  📊 Дашборд
                </NavLink>
              </li>
              <li>
                <NavLink
                  to="/categories"
                  className={({ isActive }) => isActive ? 'active' : ''}
                  onClick={closeMobileMenu}
                >
                  📁 Категории
                </NavLink>
              </li>
              <li>
                <NavLink
                  to="/questions"
                  className={({ isActive }) => isActive ? 'active' : ''}
                  onClick={closeMobileMenu}
                >
                  ❓ Вопросы
                </NavLink>
              </li>
              <li>
                <NavLink
                  to="/leaderboard"
                  className={({ isActive }) => isActive ? 'active' : ''}
                  onClick={closeMobileMenu}
                >
                  🏆 Лидеры
                </NavLink>
              </li>
              <li>
                <NavLink
                  to="/settings"
                  className={({ isActive }) => isActive ? 'active' : ''}
                  onClick={closeMobileMenu}
                >
                  ⚙️ Настройки
                </NavLink>
              </li>
            </ul>
          </nav>
          <main className="content">
            <button
              className="mobile-menu-btn"
              onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
              aria-label="Toggle menu"
            >
              ☰
            </button>
            <Routes>
              <Route path="/" element={<Dashboard />} />
              <Route path="/categories" element={<Categories />} />
              <Route path="/questions" element={<Questions />} />
              <Route path="/leaderboard" element={<Leaderboard />} />
              <Route path="/settings" element={<Settings />} />
            </Routes>
          </main>
        </div>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
