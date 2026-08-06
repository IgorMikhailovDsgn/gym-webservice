import type { ProblemDetails } from './types';

const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5080/api';

const TOKEN_KEY = 'gym_token';

export const tokenStorage = {
  get: () => localStorage.getItem(TOKEN_KEY),
  set: (token: string) => localStorage.setItem(TOKEN_KEY, token),
  clear: () => localStorage.removeItem(TOKEN_KEY),
};

/** Ошибка API с разобранным телом ProblemDetails. */
export class ApiError extends Error {
  constructor(
    public status: number,
    public problem: ProblemDetails,
  ) {
    super(problem.detail ?? problem.title ?? 'Ошибка запроса');
  }

  /** Плоский список сообщений валидации для показа пользователю. */
  get validationMessages(): string[] {
    return Object.values(this.problem.errors ?? {}).flat();
  }
}

interface RequestOptions {
  method?: string;
  body?: unknown;
  query?: Record<string, string | number | undefined | null>;
}

export async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { method = 'GET', body, query } = options;

  const url = new URL(BASE_URL + path, window.location.origin);

  if (query) {
    for (const [key, value] of Object.entries(query)) {
      if (value !== undefined && value !== null && value !== '') {
        url.searchParams.set(key, String(value));
      }
    }
  }

  const headers: Record<string, string> = { 'Content-Type': 'application/json' };

  const token = tokenStorage.get();
  if (token) headers.Authorization = `Bearer ${token}`;

  const response = await fetch(url.toString(), {
    method,
    headers,
    body: body === undefined ? undefined : JSON.stringify(body),
  });

  // 401 означает, что токен истёк или недействителен: чистим и уходим на вход.
  if (response.status === 401) {
    tokenStorage.clear();
    if (!window.location.pathname.startsWith('/login')) {
      window.location.href = '/login';
    }
    throw new ApiError(401, { title: 'Требуется вход' });
  }

  if (response.status === 204) return undefined as T;

  const text = await response.text();
  const payload = text ? JSON.parse(text) : null;

  if (!response.ok) {
    throw new ApiError(response.status, (payload ?? {}) as ProblemDetails);
  }

  return payload as T;
}
