import axios from 'axios';
import { toast } from 'react-hot-toast';

const API_BASE_URL = (import.meta as any).env.VITE_API_BASE_URL ;

export const axiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

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

axiosInstance.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('accessToken');

      toast.error('Connection Time out, please login again');

      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default axiosInstance;
