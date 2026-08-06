import { Tag } from 'antd';
import type { ClientStatus, TicketStatus } from '../api/types';

const CLIENT_LABELS: Record<ClientStatus, { text: string; color: string }> = {
  active: { text: 'Активен', color: 'green' },
  inactive: { text: 'Неактивен', color: 'default' },
  blocked: { text: 'Заблокирован', color: 'red' },
};

const TICKET_LABELS: Record<TicketStatus, { text: string; color: string }> = {
  active: { text: 'Действует', color: 'green' },
  pending: { text: 'Не начался', color: 'blue' },
  expired: { text: 'Просрочен', color: 'red' },
  exhausted: { text: 'Исчерпан', color: 'orange' },
  cancelled: { text: 'Отменён', color: 'default' },
};

export function ClientStatusTag({ status }: { status: ClientStatus }) {
  const label = CLIENT_LABELS[status] ?? { text: status, color: 'default' };
  return <Tag color={label.color}>{label.text}</Tag>;
}

export function TicketStatusTag({ status }: { status: TicketStatus }) {
  const label = TICKET_LABELS[status] ?? { text: status, color: 'default' };
  return <Tag color={label.color}>{label.text}</Tag>;
}
