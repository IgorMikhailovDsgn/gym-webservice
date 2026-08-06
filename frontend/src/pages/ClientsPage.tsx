import { useCallback, useEffect, useState } from 'react';
import { Button, Card, Input, Select, Space, Table, message } from 'antd';
import { useNavigate } from 'react-router-dom';
import { clientsApi } from '../api/endpoints';
import { ClientStatusTag } from '../components/StatusTag';
import { ClientFormModal } from '../components/ClientFormModal';
import type { ClientModel } from '../api/types';

export function ClientsPage() {
  const navigate = useNavigate();

  const [data, setData] = useState<ClientModel[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);

  const [search, setSearch] = useState('');
  const [status, setStatus] = useState<string | undefined>();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<ClientModel | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const result = await clientsApi.search({ search, status, page, pageSize });
      setData(result.items);
      setTotal(result.totalCount);
    } catch {
      message.error('Не удалось загрузить список клиентов');
    } finally {
      setLoading(false);
    }
  }, [search, status, page, pageSize]);

  // Поиск с задержкой: запрос уходит через 400 мс после последнего нажатия,
  // иначе на каждую букву летел бы отдельный запрос.
  useEffect(() => {
    const timer = setTimeout(load, 400);
    return () => clearTimeout(timer);
  }, [load]);

  return (
    <Card
      title="Клиенты"
      extra={
        <Button type="primary" onClick={() => { setEditing(null); setModalOpen(true); }}>
          Добавить клиента
        </Button>
      }
    >
      <Space className="mb-4" wrap>
        <Input.Search
          allowClear
          placeholder="Поиск по ФИО"
          style={{ width: 280 }}
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
        />

        <Select
          allowClear
          placeholder="Статус"
          style={{ width: 180 }}
          value={status}
          onChange={(value) => { setStatus(value); setPage(1); }}
          options={[
            { value: 'active', label: 'Активен' },
            { value: 'inactive', label: 'Неактивен' },
            { value: 'blocked', label: 'Заблокирован' },
          ]}
        />
      </Space>

      <Table<ClientModel>
        rowKey="id"
        dataSource={data}
        loading={loading}
        pagination={{
          current: page,
          pageSize,
          total,
          showSizeChanger: true,
          showTotal: (t) => `Всего: ${t}`,
          onChange: (nextPage, nextSize) => { setPage(nextPage); setPageSize(nextSize); },
        }}
        columns={[
          { title: 'Фамилия', dataIndex: 'lastName' },
          { title: 'Имя', dataIndex: 'firstName' },
          { title: 'Телефон', dataIndex: 'phone' },
          {
            title: 'Статус',
            dataIndex: 'status',
            render: (value) => <ClientStatusTag status={value} />,
          },
          {
            title: '',
            width: 200,
            render: (_, record) => (
              <Space>
                <Button size="small" onClick={() => navigate(`/clients/${record.id}`)}>
                  Открыть
                </Button>
                <Button
                  size="small"
                  onClick={() => { setEditing(record); setModalOpen(true); }}
                >
                  Изменить
                </Button>
              </Space>
            ),
          },
        ]}
      />

      <ClientFormModal
        open={modalOpen}
        client={editing}
        onClose={() => setModalOpen(false)}
        onSaved={load}
      />
    </Card>
  );
}
