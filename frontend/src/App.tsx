import { Navigate, Route, Routes } from 'react-router-dom';
import { useAuth } from './context/AuthContext';
import AppLayout from './pages/AppLayout';
import CooQueuePage from './pages/CooQueuePage';
import ErDetailPage from './pages/ErDetailPage';
import ErFormPage from './pages/ErFormPage';
import IndexPage from './pages/IndexPage';
import LoginPage from './pages/LoginPage';
import VerifyQueuePage from './pages/VerifyQueuePage';

function Protected({ children }: { children: React.ReactNode }) {
  const { user, loading } = useAuth();
  if (loading) return <div className="boot">Loading…</div>;
  if (!user) return <Navigate to="/login" replace />;
  return children;
}

function RoleHome() {
  const { user } = useAuth();
  if (!user) return null;
  if (['Safety', 'DeptHead', 'QA'].includes(user.role)) {
    return <Navigate to="/verify" replace />;
  }
  if (user.role === 'COO') {
    return <Navigate to="/coo" replace />;
  }
  return <IndexPage />;
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route
        path="/"
        element={
          <Protected>
            <AppLayout />
          </Protected>
        }
      >
        <Route index element={<RoleHome />} />
        <Route path="ers/new" element={<ErFormPage mode="create" />} />
        <Route path="ers/:id" element={<ErDetailPage />} />
        <Route path="ers/:id/edit" element={<ErFormPage mode="edit" />} />
        <Route path="verify" element={<VerifyQueuePage />} />
        <Route path="coo" element={<CooQueuePage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
