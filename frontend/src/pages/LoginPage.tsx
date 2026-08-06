import { useState } from 'react';
import { Button, Card, Form, Input, Alert, Typography } from 'antd';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { ApiError } from '../api/client';

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const onFinish = async (values: { username: string; password: string }) => {
    setLoading(true);
    setError(null);
    try {
      await login(values.username, values.password);
      navigate('/clients', { replace: true });
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Не удалось войти');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-100 p-4">
      <Card className="w-full max-w-sm">
        <Typography.Title level={3} className="text-center">GymManager</Typography.Title>

        {error && <Alert type="error" message={error} className="mb-4" showIcon />}

        <Form layout="vertical" onFinish={onFinish} initialValues={{ username: 'admin' }}>
          <Form.Item
            name="username"
            label="Логин"
            rules={[{ required: true, message: 'Введите логин' }]}
          >
            <Input autoFocus />
          </Form.Item>

          <Form.Item
            name="password"
            label="Пароль"
            rules={[{ required: true, message: 'Введите пароль' }]}
          >
            <Input.Password />
          </Form.Item>

          <Button type="primary" htmlType="submit" block loading={loading}>
            Войти
          </Button>
        </Form>
      </Card>
    </div>
  );
}
