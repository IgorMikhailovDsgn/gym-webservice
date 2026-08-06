// Типы повторяют контракты бэкенда. Держим их в одном файле,
// чтобы при изменении API было понятно, что править.

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export type ClientStatus = 'active' | 'inactive' | 'blocked';

export interface ClientModel {
  id: string;
  lastName: string;
  firstName: string;
  phone: string;
  status: ClientStatus;
}

export interface CreateClientCommand {
  lastName: string;
  firstName: string;
  middleName: string | null;
  phone: string;
  email: string | null;
}

export interface UpdateClientCommand extends CreateClientCommand {
  status: ClientStatus;
}

export type TicketStatus =
  | 'active' | 'pending' | 'expired' | 'exhausted' | 'cancelled';

export interface TicketModel {
  id: string;
  clientId: string;
  ticketTypeName: string;
  dateStart: string;
  dateEnd: string;
  visitsLimit: number | null;
  visitsUsed: number;
  visitsRemaining: number | null;
  status: TicketStatus;
}

export interface CreateTicketCommand {
  clientId: string;
  ticketTypeId: string;
  dateStart: string;
}

export interface VisitModel {
  id: string;
  ticketId: string;
  visitedAt: string;
  trainerId: string | null;
  userId: string;
  visitsRemaining: number | null;
}

export interface AuthenticatedUser {
  id: string;
  username: string;
  fullName: string;
  token: string;
}

/** Ошибка от API в формате ProblemDetails (RFC 7807). */
export interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  code?: string;
  errors?: Record<string, string[]>;
}
