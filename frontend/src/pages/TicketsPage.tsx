import { useCallback, useEffect, useState } from 'react';
import { Card, DatePicker, Select, Space, Table, message } from 'antd';
import { useNavigate } from 'react-router-dom';
import dayjs, { type Dayjs } from 'dayjs';
import { ticketsApi } from '../api/endpoints';
import { TicketStatusTag } from '../components/StatusTag';
import type { TicketModel } from '../api/types';

export function TicketsPage() {
  const navigate = useNavigate();

  const [data, setData] = useState<TicketModel[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);

  const [status, setStatus] = useState<string | undefined>();
  const [activeOn, setActiveOn] = useState<Dayjs | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const result = await ticketsApi.search({
        status,
        activeOn: activeOn ? activeOn.format('YYYY-MM-DD') : undefined,
        page,
        pageSize,
      });
      setData(result.items);
      setTotal(result.totalCount);
    } catch {
      message.error('Не удалось загрузить абонементы');
    } finally {
      setLoading(false);
    }
  }, [status, activeOn, page, pageSize]);

  useEffect(() => { load(); }, [load]);

  return (
    <Card title="Абонементы">
      <Space className="mb-4" wrap>
        <Select
          allowClear
          placeholder="Статус"
          style={{ width: 200 }}
          value={status}
          onChange={(value) => { setStatus(value); setPage(1); }}
          options={[
            { value: 'active', label: 'Действует' },
            { value: 'pending', label: 'Не начался' },
            { value: 'expired', label: 'Просрочен' },
            { value: 'exhausted', label: 'Исчерпан' },
            { value: 'cancelled', label: 'Отменён' },
          ]}
        />

        <DatePicker
          placeholder="Действующие на дату"
          format="DD.MM.YYYY"
          style={{ width: 220 }}
          value={activeOn}
          onChange={(value) => { setActiveOn(value); setPage(1); }}
        />
      </Space>

      <Table<TicketModel>
        rowKey="id"
        dataSource={data}
        loading={loading}
        onRow={(record) => ({
          onClick: () => navigate(`/clients/${record.clientId}`),
          style: { cursor: 'pointer' },
        })}
        pagination={{
          current: page,
          pageSize,
          total,
          showSizeChanger: true,
          showTotal: (t) => `Всего: ${t}`,
          onChange: (nextPage, nextSize) => { setPage(nextPage); setPageSize(nextSize); },
        }}
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
              r.visitsLimit === null ? 'Безлимит' : `${r.visitsUsed} из ${r.visitsLimit}`,
          },
          {
            title: 'Статус',
            dataIndex: 'status',
            render: (value) => <TicketStatusTag status={value} />,
          },
        ]}
      />
    </Card>
  );
}
