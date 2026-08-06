import { useEffect, useState } from 'react';
import { Modal, Form, Input, Select, Alert } from 'antd';
import { clientsApi } from '../api/endpoints';
import { ApiError } from '../api/client';
import type { ClientModel } from '../api/types';

interface Props {
  open: boolean;
  client: ClientModel | null;
  onClose: () => void;
  onSaved: () => void;
}

export function ClientFormModal({ open, client, onClose, onSaved }: Props) {
  const [form] = Form.useForm();
  const [errors, setErrors] = useState<string[]>([]);
  const [saving, setSaving] = useState(false);

  const isEdit = client !== null;

  useEffect(() => {
    if (!open) return;
    setErrors([]);
    if (client) {
      form.setFieldsValue({ ...client, middleName: null, email: null });
    } else {
      form.resetFields();
    }
  }, [open, client, form]);

  const handleOk = async () => {
    const values = await form.validateFields();
    setSaving(true);
    setErrors([]);
    try {
      const payload = {
        lastName: values.lastName,
        firstName: values.firstName,
        middleName: values.middleName || null,
        phone: values.phone,
        email: values.email || null,
      };

      if (isEdit) {
        await clientsApi.update(client.id, { ...payload, status: values.status });
      } else {
        await clientsApi.create(payload);
      }
      onSaved();
      onClose();
    } catch (e) {
      if (e instanceof ApiError) {
        const messages = e.validationMessages;
        setErrors(messages.length > 0 ? messages : [e.message]);
      } else {
        setErrors(['Не удалось сохранить клиента']);
      }
    } finally {
      setSaving(false);
    }
  };

  return (
    <Modal
      open={open}
      title={isEdit ? 'Редактирование клиента' : 'Новый клиент'}
      onCancel={onClose}
      onOk={handleOk}
      confirmLoading={saving}
      okText="Сохранить"
      cancelText="Отмена"
      destroyOnClose
    >
      {errors.length > 0 && (
        <Alert
          type="error"
          className="mb-4"
          showIcon
          message={<ul className="m-0 pl-4">{errors.map((m) => <li key={m}>{m}</li>)}</ul>}
        />
      )}

      <Form form={form} layout="vertical" initialValues={{ status: 'active' }}>
        <Form.Item name="lastName" label="Фамилия" rules={[{ required: true, message: 'Обязательное поле' }]}>
          <Input />
        </Form.Item>

        <Form.Item name="firstName" label="Имя" rules={[{ required: true, message: 'Обязательное поле' }]}>
          <Input />
        </Form.Item>

        <Form.Item name="middleName" label="Отчество">
          <Input />
        </Form.Item>

        <Form.Item name="phone" label="Телефон" rules={[{ required: true, message: 'Обязательное поле' }]}>
          <Input placeholder="+7 (999) 123-45-67" />
        </Form.Item>

        <Form.Item name="email" label="Email" rules={[{ type: 'email', message: 'Некорректный адрес' }]}>
          <Input />
        </Form.Item>

        {isEdit && (
          <Form.Item name="status" label="Статус">
            <Select
              options={[
                { value: 'active', label: 'Активен' },
                { value: 'inactive', label: 'Неактивен' },
                { value: 'blocked', label: 'Заблокирован' },
              ]}
            />
          </Form.Item>
        )}
      </Form>
    </Modal>
  );
}
