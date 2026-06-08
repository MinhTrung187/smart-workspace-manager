import { Routes, Route, Navigate } from 'react-router';
import { Toaster } from 'react-hot-toast';
import Login from './pages/Login';
import Register from './pages/Register';
import Dashboard from './pages/Dashboard';
import WorkspaceDetail from './pages/WorkspaceDetail';
import BoardDetail from './pages/BoardDetail';

export default function App() {
  return (
    <>
      <Toaster position="top-right" reverseOrder={false} />
      <Routes>
        <Route path="/" element={<Navigate to="/login" replace />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/workspaces/:id" element={<WorkspaceDetail />} />
        <Route path="/workspaces/:workspaceId/boards/:boardId" element={<BoardDetail />} />
      </Routes>
    </>
  );
}

