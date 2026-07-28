import { Navigate, Route, Routes } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import { AuthProvider } from './auth/AuthContext';
import { AppLayout } from './components/AppLayout';
import { LoginPage, ProtectedRoute } from './pages/LoginPage';
import { DashboardPage } from './pages/DashboardPage';
import { RequestListPage } from './pages/RequestListPage';
import { EngineeringRequestFormPage } from './pages/EngineeringRequestFormPage';
import { RequestDetailPage } from './pages/RequestDetailPage';
import { NotificationsPage, VerificationQueuePage } from './pages/VerificationQueuePage';
import './index.css';
import './components/layout.css';

export default function App() {
  return (
    <AuthProvider>
      <Toaster position="top-right" />
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route
          path="/"
          element={
            <ProtectedRoute>
              <AppLayout />
            </ProtectedRoute>
          }
        >
          <Route index element={<Navigate to="/dashboard" replace />} />
          <Route path="dashboard" element={<DashboardPage />} />
          <Route
            path="requests"
            element={
              <ProtectedRoute roles={['Requester']}>
                <RequestListPage mode="mine" />
              </ProtectedRoute>
            }
          />
          <Route
            path="requests/new"
            element={
              <ProtectedRoute roles={['Requester']}>
                <EngineeringRequestFormPage mode="create" />
              </ProtectedRoute>
            }
          />
          <Route
            path="requests/:id/edit"
            element={
              <ProtectedRoute roles={['Requester']}>
                <EngineeringRequestFormPage mode="edit" />
              </ProtectedRoute>
            }
          />
          <Route path="requests/:id" element={<RequestDetailPage />} />
          <Route
            path="verify/safety"
            element={
              <ProtectedRoute roles={['Safety']}>
                <VerificationQueuePage stage="safety" title="Safety Verification Queue" />
              </ProtectedRoute>
            }
          />
          <Route
            path="verify/department-head"
            element={
              <ProtectedRoute roles={['DepartmentHead']}>
                <VerificationQueuePage stage="department-head" title="Department Head Verification Queue" />
              </ProtectedRoute>
            }
          />
          <Route
            path="verify/qa"
            element={
              <ProtectedRoute roles={['QA']}>
                <VerificationQueuePage stage="qa" title="QA Verification Queue" />
              </ProtectedRoute>
            }
          />
          <Route
            path="coo"
            element={
              <ProtectedRoute roles={['COO']}>
                <VerificationQueuePage stage="coo" title="COO Approval Dashboard" />
              </ProtectedRoute>
            }
          />
          <Route
            path="admin/requests"
            element={
              <ProtectedRoute roles={['Administrator']}>
                <RequestListPage mode="all" />
              </ProtectedRoute>
            }
          />
          <Route path="notifications" element={<NotificationsPage />} />
        </Route>
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </AuthProvider>
  );
}
