export interface Category {
  id: number;
  name: string;
  description?: string;
  emoji?: string;
  languageCode: string;
  translationGroupId?: string;
  categoryGroup?: string;
  questionsCount?: number;
}

export const CATEGORY_GROUPS = [
  { value: 'general', label: 'Общие' },
  { value: 'special', label: 'Специальные' },
  { value: 'popular', label: 'Популярные' },
] as const;

export interface Question {
  id: number;
  categoryId: number;
  category?: string;
  languageCode: string;
  translationGroupId?: string;
  text: string;
  correctAnswer: string;
  wrongAnswer1: string;
  wrongAnswer2: string;
  wrongAnswer3: string;
  explanation?: string;
  imageUrl?: string;
  createdAt?: string;
}

export const SUPPORTED_LANGUAGES = [
  { code: 'ru', name: 'Русский', flag: '🇷🇺' },
  { code: 'hi', name: 'हिन्दी', flag: '🇮🇳' },
  { code: 'pt', name: 'Português', flag: '🇧🇷' },
  { code: 'fa', name: 'فارسی', flag: '🇮🇷' },
  { code: 'de', name: 'Deutsch', flag: '🇩🇪' },
  { code: 'uz', name: "O'zbek", flag: '🇺🇿' },
  { code: 'en', name: 'English', flag: '🇬🇧' },
] as const;

export interface Stats {
  totalUsers: number;
  totalGames: number;
  totalQuestions: number;
  totalCategories: number;
  gamesToday: number;
}

export interface GameSetting {
  id: number;
  key: string;
  value: string;
  description?: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}

export interface LeaderboardPlayer {
  id: number;
  telegramId: number;
  username?: string;
  firstName?: string;
  lastName?: string;
  gamesPlayed: number;
  gamesWon: number;
  totalCorrectAnswers: number;
  winRate: number;
  lastActivityAt?: string;
}
