import { useCallback, useEffect, useState } from 'react';
import {
  Button, Card, Descriptions, Space, Table, Tag, message, Modal, InputNumber,
} from 'antd';
import { useNavigate, useParams } from 'react-router-dom';
import dayjs from 'dayjs';
import { clientsApi, ticketsApi, visitsApi } from '../api/endpoints';
import { ApiError } from '../api/client';
import { ClientStatusTag, TicketStatusTag } from '../components/StatusTag';
import { CreateTicketModal } from '../components/CreateTicketModal';
import type { ClientModel, TicketModel } from '../api/types';

export function ClientDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [client, setClient] = useState<ClientModel | null>(null);
  const [tickets, setTickets] = useState<TicketModel[]>([]);
  const [loading, setLoading] = useState(false);
  const [ticketModalOpen, setTicketModalOpen] = useState(false);

  const load = useCallback(async () => {
    if (!id) return;
    setLoading(true);
    try {
      const [clientData, ticketsData] = await Promise.all([
        clientsApi.getById(id),
        clientsApi.getTickets(id),
      ]);
      setClient(clientData);
      setTickets(ticketsData);
    } catch {
      message.error('Не удалось загрузить данные клиента');
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => { load(); }, [load]);

  const registerVisit = async (ticketId: string) => {
    try {
      const visit = await visitsApi.register(ticketId);
      message.success(
        visit.visitsRemaining === null
          ? 'Посещение зафиксировано'
          : `Посещение зафиксировано. Осталось: ${visit.visitsRemaining}`,
      );
      load();
    } catch (e) {
      // 409 приходит с человекочитаемой причиной отказа из бизнес-правил.
      message.error(e instanceof ApiError ? e.message : 'Не удалось зафиксировать посещение');
    }
  };

  const extendTicket = (ticket: TicketModel) => {
    let days = 30;
    Modal.confirm({
      title: 'Продление абонемента',
      content: (
        <div className="pt-2">
          <span className="mr-2">Продлить на (дней):</span>
          <InputNumber min={1} max={365} defaultValue={days} onChange={(v) => { days = v ?? 30; }} />
        </div>
      ),
      okText: 'Продлить',
      cancelText: 'Отмена',
      onOk: async () => {
        try {
          await ticketsApi.extend(ticket.id, days);
          message.success('Абонемент продлён');
          load();
        } catch (e) {
          message.error(e instanceof ApiError ? e.message : 'Не удалось продлить');
        }
      },
    });
  };

  return (
    <Space direction="vertical" size="middle" className="w-full">
      <Card
        title="Карточка клиента"
        loading={loading}
        extra={<Button onClick={() => navigate('/clients')}>К списку</Button>}
      >
        {client && (
          <Descriptions column={2}>
            <Descriptions.Item label="Фамилия">{client.lastName}</Descriptions.Item>
            <Descriptions.Item label="Имя">{client.firstName}</Descriptions.Item>
            <Descriptions.Item label="Телефон">{client.phone}</Descriptions.Item>
            <Descriptions.Item label="Статус">
              <ClientStatusTag status={client.status} />
            </Descriptions.Item>
          </Descriptions>
        )}
      </Card>

      <Card
        title="Абонементы"
        extra={
          <Button type="primary" onClick={() => setTicketModalOpen(true)}>
            Оформить абонемент
          </Button>
        }
      >
        <Table<TicketModel>
          rowKey="id"
          dataSource={tickets}
          loading={loading}
          pagination={false}
          columns={[
            { title: 'Тип', dataIndex: 'ticketTypeName' },
            {
              title: 'Период',
              render: (_, r) =>
                `${dayjs(r.dateStart).format('DD.MM.YYYY')} — ${dayjs(r.dateEnd).format('DD.MM.YYYY')}`,
            },
            {
              title: 'Посещения',
              render: (_, r) =>
                r.visitsLimit === null
                  ? <Tag>Безлимит</Tag>
                  : `${r.visitsUsed} из ${r.visitsLimit}`,
            },
            {
              title: 'Осталось',
              render: (_, r) => (r.visitsRemaining === null ? '—' : r.visitsRemaining),
            },
            {
              title: 'Статус',
              dataIndex: 'status',
              render: (value) => <TicketStatusTag status={value} />,
            },
            {
              title: '',
              width: 240,
              render: (_, r) => (
                <Space>
                  <Button
                    size="small"
                    type="primary"
                    disabled={r.status !== 'active'}
                    onClick={() => registerVisit(r.id)}
                  >
                    Отметить посещение
                  </Button>
                  <Button
                    size="small"
                    disabled={r.status === 'cancelled'}
                    onClick={() => extendTicket(r)}
                  >
                    Продлить
                  </Button>
                </Space>
              ),
            },
          ]}
        />
      </Card>

      {id && (
        <CreateTicketModal
          open={ticketModalOpen}
          clientId={id}
          onClose={() => setTicketModalOpen(false)}
          onCreated={load}
        />
      )}
    </Space>
  );
}
