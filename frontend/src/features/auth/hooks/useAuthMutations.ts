import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router';
import { loginUser, registerUser } from '../api/authApi';
import type { LoginRequest, RegisterRequest } from '../types';

export const useLoginMutation = () => {
  const navigate = useNavigate();

  return useMutation({
    mutationFn: (data: LoginRequest) => loginUser(data),
    onSuccess: (data) => {
      localStorage.setItem('accessToken', data.accessToken);
      navigate('/dashboard');
    },
  });
};

export const useRegisterMutation = () => {
  const navigate = useNavigate();

  return useMutation({
    mutationFn: (data: RegisterRequest) => registerUser(data),
    onSuccess: () => {
      navigate('/login');
    },
  });
};
