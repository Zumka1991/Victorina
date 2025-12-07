import axios from 'axios';
import type { Category, Question, Stats, GameSetting, PaginatedResponse, LeaderboardPlayer } from '../types';

// Use env variable for local development, empty string for Docker (nginx proxy)
const API_URL = import.meta.env.VITE_API_URL || '';

console.log('API_URL:', API_URL || 'Using relative path via nginx proxy');

const api = axios.create({
  baseURL: API_URL,
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

export const generateQuestions = (count: number, languages: string[], categoryName?: string, difficulty?: string) =>
  api.post<GeneratedQuestion[]>('/api/ai/generate-questions', {
    count,
    languages,
    categoryName,
    difficulty
  }).then(res => res.data);

export interface GenerationEvent {
  type: 'progress' | 'log' | 'complete' | 'error';
  data: any;
}

export const generateQuestionsStream = async (
  count: number,
  languages: string[],
  categoryName?: string,
  difficulty?: string,
  onEvent?: (event: GenerationEvent) => void
): Promise<GeneratedQuestion[]> => {
  return new Promise((resolve, reject) => {
    const url = `${API_URL}/api/ai/generate-questions-stream`;

    // We need to use fetch with POST instead of EventSource (which only supports GET)
    // Let's use a different approach with fetch and streaming
    fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        count,
        languages,
        categoryName,
        difficulty
      })
    }).then(async response => {
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const reader = response.body?.getReader();
      const decoder = new TextDecoder();
      let buffer = '';

      if (!reader) {
        throw new Error('Response body is null');
      }

      while (true) {
        const { done, value } = await reader.read();

        if (done) break;

        buffer += decoder.decode(value, { stream: true });
        const lines = buffer.split('\n\n');
        buffer = lines.pop() || '';

        for (const line of lines) {
          if (!line.trim()) continue;

          const [eventLine, dataLine] = line.split('\n');
          if (!eventLine.startsWith('event:') || !dataLine.startsWith('data:')) continue;

          const eventType = eventLine.substring(7).trim();
          const data = JSON.parse(dataLine.substring(6));

          if (eventType === 'complete') {
            resolve(data.questions);
            return;
          } else if (eventType === 'error') {
            reject(new Error(data.error || data.details || 'Unknown error'));
            return;
          } else {
            onEvent?.({ type: eventType as any, data });
          }
        }
      }
    }).catch(reject);
  });
};

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

// Translation
export interface TranslationEvent {
  type: 'progress' | 'log' | 'complete' | 'error';
  data: any;
}

export const autoTranslateQuestions = async (
  onEvent?: (event: TranslationEvent) => void
): Promise<void> => {
  return new Promise((resolve, reject) => {
    const url = `${API_URL}/api/translation/auto-translate`;

    fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      }
    }).then(async response => {
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const reader = response.body?.getReader();
      const decoder = new TextDecoder();
      let buffer = '';

      if (!reader) {
        throw new Error('Response body is null');
      }

      while (true) {
        const { done, value } = await reader.read();

        if (done) break;

        buffer += decoder.decode(value, { stream: true });
        const lines = buffer.split('\n\n');
        buffer = lines.pop() || '';

        for (const line of lines) {
          if (!line.trim()) continue;

          const [eventLine, dataLine] = line.split('\n');
          if (!eventLine.startsWith('event:') || !dataLine.startsWith('data:')) continue;

          const eventType = eventLine.substring(7).trim();
          const data = JSON.parse(dataLine.substring(6));

          if (eventType === 'complete') {
            resolve();
            return;
          } else if (eventType === 'error') {
            reject(new Error(data.error || 'Unknown error'));
            return;
          } else {
            onEvent?.({ type: eventType as any, data });
          }
        }
      }
    }).catch(reject);
  });
};

// Backup
export const exportBackup = () =>
  api.get('/api/backup/export').then(res => res.data);

export const importBackup = (backupData: any) =>
  api.post('/api/backup/import', backupData).then(res => res.data);
