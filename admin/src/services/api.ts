import axios from 'axios';
import type { Category, Question, Stats, GameSetting, PaginatedResponse, LeaderboardPlayer } from '../types';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5175';

const api = axios.create({
  baseURL: API_URL,
});

// Categories
export const getCategories = () =>
  api.get<Category[]>('/api/categories').then(res => res.data);

export const createCategory = (data: Omit<Category, 'id'>) =>
  api.post<Category>('/api/categories', data).then(res => res.data);

export const updateCategory = (id: number, data: Omit<Category, 'id'>) =>
  api.put<Category>(`/api/categories/${id}`, data).then(res => res.data);

export const deleteCategory = (id: number) =>
  api.delete(`/api/categories/${id}`);

export const getCategoryTranslations = (translationGroupId: string) =>
  api.get<Category[]>(`/api/categories/translations/${translationGroupId}`).then(res => res.data);

// Questions
export const getQuestions = (page = 1, pageSize = 20, categoryId?: number, languageCode?: string) =>
  api.get<PaginatedResponse<Question>>('/api/questions', {
    params: { page, pageSize, categoryId, languageCode }
  }).then(res => res.data);

export const createQuestion = (data: Omit<Question, 'id'>) =>
  api.post<Question>('/api/questions', data).then(res => res.data);

export const updateQuestion = (id: number, data: Omit<Question, 'id'>) =>
  api.put<Question>(`/api/questions/${id}`, data).then(res => res.data);

export const deleteQuestion = (id: number) =>
  api.delete(`/api/questions/${id}`);

export const getQuestionTranslations = (translationGroupId: string) =>
  api.get<Question[]>(`/api/questions/translations/${translationGroupId}`).then(res => res.data);

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
