import { useState } from 'react';
import { Modal, Form, Select, DatePicker, Alert } from 'antd';
import dayjs from 'dayjs';
import { ticketsApi } from '../api/endpoints';
import { ApiError } from '../api/client';

// ВРЕМЕННО: идентификаторы типов абонементов взяты из сид-данных.
// На бэкенде нет эндпоинта GET /api/ticket-types — его стоит добавить,
// иначе список придётся править руками при любом изменении справочника.
const TICKET_TYPES = [
  { value: 'a0000000-0000-0000-0000-000000000001', label: 'Разовое посещение' },
  { value: 'a0000000-0000-0000-0000-000000000002', label: 'Месячный, 8 занятий' },
  { value: 'a0000000-0000-0000-0000-000000000003', label: 'Месячный, 12 занятий' },
  { value: 'a0000000-0000-0000-0000-000000000004', label: 'Месячный безлимит' },
  { value: 'a0000000-0000-0000-0000-000000000005', label: 'Годовой безлимит' },
];

interface Props {
  open: boolean;
  clientId: string;
  onClose: () => void;
  onCreated: () => void;
}

export function CreateTicketModal({ open, clientId, onClose, onCreated }: Props) {
  const [form] = Form.useForm();
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const handleOk = async () => {
    const values = await form.validateFields();
    setSaving(true);
    setError(null);
    try {
      await ticketsApi.create({
        clientId,
        ticketTypeId: values.ticketTypeId,
        dateStart: values.dateStart.format('YYYY-MM-DD'),
      });
      onCreated();
      onClose();
      form.resetFields();
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Не удалось оформить абонемент');
    } finally {
      setSaving(false);
    }
  };

  return (
    <Modal
      open={open}
      title="Оформление абонемента"
      onCancel={onClose}
      onOk={handleOk}
      confirmLoading={saving}
      okText="Оформить"
      cancelText="Отмена"
      destroyOnClose
    >
      {error && <Alert type="error" message={error} className="mb-4" showIcon />}

      <Form form={form} layout="vertical" initialValues={{ dateStart: dayjs() }}>
        <Form.Item
          name="ticketTypeId"
          label="Тип абонемента"
          rules={[{ required: true, message: 'Выберите тип' }]}
        >
          <Select options={TICKET_TYPES} />
        </Form.Item>

        <Form.Item
          name="dateStart"
          label="Дата начала"
          rules={[{ required: true, message: 'Укажите дату' }]}
        >
          <DatePicker className="w-full" format="DD.MM.YYYY" />
        </Form.Item>
      </Form>
    </Modal>
  );
}
