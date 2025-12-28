import api, { ApiResponse } from './api';
import { AuthResponse, LoginRequest, RegisterRequest } from '../types';

export const authService = {
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    const response = await api.post<ApiResponse<AuthResponse>>('/auth/login', credentials);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Login failed');
    }
    return response.data.data;
  },

  async register(data: RegisterRequest): Promise<AuthResponse> {
    const response = await api.post<ApiResponse<AuthResponse>>('/auth/register', data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Registration failed');
    }
    return response.data.data;
  },

  async getCurrentUser(): Promise<AuthResponse> {
    const response = await api.get<ApiResponse<AuthResponse>>('/auth/me');
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to get user');
    }
    return response.data.data;
  },

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },
};
