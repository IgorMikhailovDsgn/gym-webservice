import { request } from './client';
import type {
  AuthenticatedUser, ClientModel, CreateClientCommand, CreateTicketCommand,
  PagedResult, TicketModel, UpdateClientCommand, VisitModel,
} from './types';

export const authApi = {
  login: (username: string, password: string) =>
    request<AuthenticatedUser>('/auth/login', {
      method: 'POST',
      body: { username, password },
    }),
};

export const clientsApi = {
  search: (params: { search?: string; status?: string; page: number; pageSize: number }) =>
    request<PagedResult<ClientModel>>('/clients', { query: params }),

  getById: (id: string) => request<ClientModel>(`/clients/${id}`),

  getTickets: (id: string) => request<TicketModel[]>(`/clients/${id}/tickets`),

  create: (command: CreateClientCommand) =>
    request<ClientModel>('/clients', { method: 'POST', body: command }),

  update: (id: string, command: UpdateClientCommand) =>
    request<ClientModel>(`/clients/${id}`, { method: 'PUT', body: command }),
};

export const ticketsApi = {
  search: (params: {
    clientId?: string; status?: string; activeOn?: string;
    page: number; pageSize: number;
  }) => request<PagedResult<TicketModel>>('/tickets', { query: params }),

  create: (command: CreateTicketCommand) =>
    request<TicketModel>('/tickets', { method: 'POST', body: command }),

  extend: (id: string, days: number) =>
    request<TicketModel>(`/tickets/${id}/extend`, { method: 'POST', body: { days } }),
};

export const visitsApi = {
  register: (ticketId: string, trainerId: string | null = null) =>
    request<VisitModel>('/visits', {
      method: 'POST',
      body: { ticketId, trainerId },
    }),
};
