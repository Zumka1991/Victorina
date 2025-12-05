import axios from 'axios';
import type { Category, Question, Stats, GameSetting, PaginatedResponse, LeaderboardPlayer } from '../types';

// Use runtime config if available, otherwise fall back to build-time env var
// Empty string means use relative path (same host via nginx proxy)
const runtimeUrl = (window as any).APP_CONFIG?.API_URL;
let API_URL = '';

if (runtimeUrl !== undefined && runtimeUrl !== '__API_URL__') {
  // Runtime config is set
  API_URL = runtimeUrl;
} else {
  // Fall back to build-time env var
  API_URL = import.meta.env.VITE_API_URL || '';
}

console.log('Runtime URL:', runtimeUrl);
console.log('API_URL configured as:', API_URL || '(empty = relative path via nginx proxy)');

const api = axios.create({
  baseURL: API_URL || undefined,
});

// Add response interceptor for better error handling
api.interceptors.response.use(
  response => response,
  error => {
    console.error('API Error:', error.response?.data || error.message);
    return Promise.reject(error);
  }
);

// Categories
export const getCategories = () =>
  api.get<Category[]>('/api/categories').then(res => {
    console.log('Categories response:', res.data);
    return Array.isArray(res.data) ? res.data : [];
  });

export const createCategory = (data: Omit<Category, 'id'>) =>
  api.post<Category>('/api/categories', data).then(res => res.data);

export const updateCategory = (id: number, data: Omit<Category, 'id'>) =>
  api.put<Category>(`/api/categories/${id}`, data).then(res => res.data);

export const deleteCategory = (id: number) =>
  api.delete(`/api/categories/${id}`);

export const getCategoryTranslations = (translationGroupId: string) =>
  api.get<Category[]>(`/api/categories/translations/${translationGroupId}`).then(res => res.data);

// Questions
export const getQuestions = (page = 1, pageSize = 20, categoryId?: number, languageCode?: string, search?: string) =>
  api.get<PaginatedResponse<Question>>('/api/questions', {
    params: { page, pageSize, categoryId, languageCode, search }
  }).then(res => res.data);

export const createQuestion = (data: Omit<Question, 'id'>) =>
  api.post<Question>('/api/questions', data).then(res => res.data);

export const updateQuestion = (id: number, data: Omit<Question, 'id'>) =>
  api.put<Question>(`/api/questions/${id}`, data).then(res => res.data);

export const deleteQuestion = (id: number) =>
  api.delete(`/api/questions/${id}`);

export const getQuestionTranslations = (translationGroupId: string) =>
  api.get<Question[]>(`/api/questions/translations/${translationGroupId}`).then(res => res.data);

// AI Generation
export interface GeneratedQuestion {
  translationGroupId: string;
  languageCode: string;
  text: string;
  correctAnswer: string;
  wrongAnswer1: string;
  wrongAnswer2: string;
  wrongAnswer3: string;
  explanation?: string;
  difficulty: string;
}

export const generateQuestions = (count: number, languages: string[], categoryName?: string) =>
  api.post<GeneratedQuestion[]>('/api/ai/generate-questions', {
    count,
    languages,
    categoryName
  }).then(res => res.data);

// Stats
export const getStats = () =>
  api.get<Stats>('/api/stats').then(res => res.data);

// Settings
export const getSettings = () =>
  api.get<GameSetting[]>('/api/settings').then(res => res.data);

export const updateSetting = (key: string, value: string) =>
  api.put(`/api/settings/${key}`, { value }).then(res => res.data);

// Seed
export const seedData = () =>
  api.post('/api/seed').then(res => res.data);

export const resetSeedData = () =>
  api.post('/api/seed/reset').then(res => res.data);

// Leaderboard
export const getLeaderboard = (sort = 'wins', page = 1, pageSize = 20) =>
  api.get<PaginatedResponse<LeaderboardPlayer>>('/api/leaderboard', {
    params: { sort, page, pageSize }
  }).then(res => res.data);

// Upload
export const uploadImage = async (file: File): Promise<{ url: string; fileName: string }> => {
  const formData = new FormData();
  formData.append('file', file);
  const res = await api.post<{ url: string; fileName: string }>('/api/upload', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  });
  return { url: res.data.url, fileName: res.data.fileName };
};

export const getFullImageUrl = (path: string | undefined | null): string | undefined => {
  if (!path) return undefined;
  if (path.startsWith('http')) return path;
  return `${API_URL}${path}`;
};
