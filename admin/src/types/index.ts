export interface Category {
  id: number;
  name: string;
  description?: string;
  emoji?: string;
  questionsCount?: number;
}

export interface Question {
  id: number;
  categoryId: number;
  category?: string;
  text: string;
  correctAnswer: string;
  wrongAnswer1: string;
  wrongAnswer2: string;
  wrongAnswer3: string;
  explanation?: string;
  imageUrl?: string;
  createdAt?: string;
}

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
