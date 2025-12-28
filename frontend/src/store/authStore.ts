import { create } from 'zustand';
import { User, UserRole } from '../types';
import { authService } from '../services/authService';

interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;

  login: (email: string, password: string) => Promise<void>;
  register: (data: {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    phoneNumber?: string;
    role: UserRole;
  }) => Promise<void>;
  logout: () => void;
  checkAuth: () => Promise<void>;
  clearError: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  token: localStorage.getItem('token'),
  isAuthenticated: !!localStorage.getItem('token'),
  isLoading: false,
  error: null,

  login: async (email: string, password: string) => {
    set({ isLoading: true, error: null });
    try {
      const response = await authService.login({ email, password });

      localStorage.setItem('token', response.token);
      localStorage.setItem('user', JSON.stringify({
        id: response.userId,
        email: response.email,
        firstName: response.firstName,
        lastName: response.lastName,
        role: response.role,
      }));

      set({
        user: {
          id: response.userId,
          email: response.email,
          firstName: response.firstName,
          lastName: response.lastName,
          role: response.role,
        },
        token: response.token,
        isAuthenticated: true,
        isLoading: false,
      });
    } catch (error: any) {
      set({
        error: error.response?.data?.message || 'Login failed',
        isLoading: false,
      });
      throw error;
    }
  },

  register: async (data) => {
    set({ isLoading: true, error: null });
    try {
      const response = await authService.register(data);

      localStorage.setItem('token', response.token);
      localStorage.setItem('user', JSON.stringify({
        id: response.userId,
        email: response.email,
        firstName: response.firstName,
        lastName: response.lastName,
        role: response.role,
      }));

      set({
        user: {
          id: response.userId,
          email: response.email,
          firstName: response.firstName,
          lastName: response.lastName,
          role: response.role,
        },
        token: response.token,
        isAuthenticated: true,
        isLoading: false,
      });
    } catch (error: any) {
      let errorMessage = 'Registration failed';
      
      // Backend'den gelen validation hatalarını parse et
      if (error.response?.data) {
        const data = error.response.data;
        
        // Eğer errors array varsa, tüm hataları birleştir
        if (data.errors && Array.isArray(data.errors) && data.errors.length > 0) {
          errorMessage = data.errors.join('. ');
        } else if (data.message) {
          errorMessage = data.message;
        }
      } else if (error.message) {
        errorMessage = error.message;
      }
      
      set({
        error: errorMessage,
        isLoading: false,
      });
      throw error;
    }
  },

  logout: () => {
    authService.logout();
    set({
      user: null,
      token: null,
      isAuthenticated: false,
      error: null,
    });
  },

  checkAuth: async () => {
    const token = localStorage.getItem('token');
    if (!token) {
      set({ isAuthenticated: false, user: null, token: null });
      return;
    }

    set({ isLoading: true });
    try {
      const response = await authService.getCurrentUser();
      set({
        user: {
          id: response.userId,
          email: response.email,
          firstName: response.firstName,
          lastName: response.lastName,
          role: response.role,
        },
        token: response.token,
        isAuthenticated: true,
        isLoading: false,
      });
    } catch (error) {
      authService.logout();
      set({
        user: null,
        token: null,
        isAuthenticated: false,
        isLoading: false,
      });
    }
  },

  clearError: () => set({ error: null }),
}));
