import axios from 'axios';
import { toast } from 'react-hot-toast';

const API_BASE_URL = (import.meta as any).env.VITE_API_BASE_URL ;

export const axiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to attach JWT token
axiosInstance.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor to handle expired tokens or unauthenticated states (401)
axiosInstance.interceptors.response.use(
  (response) => {
    // Check if the backend returns a success message or a notification string
    const successMsg = response.data?.message || response.data?.successMessage;
    if (successMsg && typeof successMsg === 'string') {
      toast.success(successMsg);
    }
    return response;
  },
  (error) => {
    // Extract error message first
    let errorMessage = '';
    if (error.response?.data) {
      if (typeof error.response.data === 'string') {
        errorMessage = error.response.data;
      } else if (error.response.data.message && typeof error.response.data.message === 'string') {
        errorMessage = error.response.data.message;
      } else if (error.response.data.title && typeof error.response.data.title === 'string') {
        errorMessage = error.response.data.title;
      }
    }

    if (error.response?.status === 401) {
      const lowerMessage = errorMessage.toLowerCase();
      const isPermissionIssue = lowerMessage.includes('permission') || lowerMessage.includes('not authorized') || lowerMessage.includes('forbidden');

      if (isPermissionIssue) {
        // Log the message as a toast instead of aggressively routing them out
        toast.error(errorMessage || 'You do not have permission to perform this action');
      } else {
        // Clear expired authentication token and user data
        localStorage.removeItem('accessToken');

        // Trigger global toast notification
        toast.error(errorMessage || 'Connection Time out, please login again');

        // Force redirect to login page with a timeout buffer so the toast actually renders
        setTimeout(() => {
          window.location.href = '/login';
        }, 150);
      }
    } else {
      // For all other exceptions (400, 403, 404, 500)
      const fallbackMsg = errorMessage || error.message || 'An error occurred';
      toast.error(fallbackMsg);
    }
    return Promise.reject(error);
  }
);

export default axiosInstance;
