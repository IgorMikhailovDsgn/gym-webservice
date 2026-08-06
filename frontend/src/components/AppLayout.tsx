import { Layout, Menu, Button } from 'antd';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

const { Header, Content } = Layout;

export function AppLayout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const selectedKey = location.pathname.startsWith('/tickets') ? '/tickets' : '/clients';

  return (
    <Layout style={{ minHeight: '100vh' }}>
      {/* Цвета заданы инлайново, а не Tailwind-классами: у AntD собственные
          стили с более высокой специфичностью, и text-white на её компонентах
          не срабатывает. Tailwind оставляем на раскладку. */}
      <Header
        style={{
          display: 'flex',
          alignItems: 'center',
          gap: 24,
          paddingInline: 24,
          background: '#001529',
        }}
      >
        <span
          style={{
            color: '#fff',
            fontSize: 18,
            fontWeight: 600,
            whiteSpace: 'nowrap',
          }}
        >
          GymManager
        </span>

        <Menu
          theme="dark"
          mode="horizontal"
          selectedKeys={[selectedKey]}
          style={{ flex: 1, minWidth: 0, background: 'transparent', borderBottom: 'none' }}
          onClick={({ key }) => navigate(key)}
          items={[
            { key: '/clients', label: 'Клиенты' },
            { key: '/tickets', label: 'Абонементы' },
          ]}
        />

        <span style={{ color: 'rgba(255,255,255,0.75)', whiteSpace: 'nowrap' }}>
          {user?.fullName}
        </span>

        <Button size="small" onClick={() => { logout(); navigate('/login'); }}>
          Выйти
        </Button>
      </Header>

      <Content style={{ padding: 24 }}>
        <Outlet />
      </Content>
    </Layout>
  );
}
